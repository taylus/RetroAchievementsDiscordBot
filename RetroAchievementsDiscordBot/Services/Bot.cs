using Serilog;

namespace RetroAchievementsDiscordBot;

public class Bot(BotOptions options, IDatabaseClient dbClient, IRetroAchievementsClient raClient, IDiscordRestApiClient discordClient)
{
    private readonly BotOptions options = options;
    private readonly IDatabaseClient dbClient = dbClient;
    private readonly IRetroAchievementsClient raClient = raClient;
    private readonly IDiscordRestApiClient discordClient = discordClient;

    public async Task PollForAchievementsAndPostToDiscord()
    {
        var users = await dbClient.GetUsersAsync();
        Log.Debug("  DB: Got {count} user(s)", users.Count());

        foreach (var user in users)
        {
            //get any achievements this user has unlocked since we last checked
            var to = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Log.Debug("  RA: Getting achievements for {user} between [{from}] and [{to}]...", user.Name,
                DateTimeOffset.FromUnixTimeSeconds(user.LastUpdated).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"),
                DateTimeOffset.FromUnixTimeSeconds(to).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));
            var achievements = await raClient.GetRecentAchievementsForUserAsync(user.Ulid, user.LastUpdated, to);

            //post any new achievements to Discord
            var progressByGame = new Dictionary<int, GameInfoAndUserProgress>();
            foreach (var achievement in achievements)
            {
                Log.Information("  RA: {user} unlocked {title} for {gameTitle} on [{date}]", user.Name, achievement.Title,
                    achievement.GameTitle, achievement.DateTimeOffset.ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));

                //check the database to see if we've already posted this
                if (await dbClient.AchievementUnlockExistsAsync(user.Ulid, achievement.AchievementId))
                {
                    Log.Debug("  DB: Achievement {achievementId} for {user} already exists, skipping...", achievement.AchievementId, user.Name);
                    continue;
                }

                foreach (var channelId in options.Discord.ChannelIds)
                {
                    Log.Information("  Discord: Posting to channel {channelId}: {user} unlocked {title}!", channelId, user.Name, achievement.Title);
                    await discordClient.PostAchievementUnlockAsync(achievement, user, channelId);
                    await Task.Delay(options.RateLimitDelayInMilliseconds);
                }

                //check if this achievement means the user has beaten or mastered the game
                await HandleBeatenOrMastered(user, achievement, progressByGame);

                //save this unlock to the database so we don't post it again
                Log.Debug("  DB: Saving unlock of {achievement} for {user}...", achievement.Title, user.Name);
                await dbClient.SaveAchievementUnlockAsync(user.Ulid, achievement);
            }

            //update the user's last updated time so next run we only get new achievements
            Log.Debug("  DB: Setting LastUpdated for {user} to {to}...", user.Name, DateTimeOffset.FromUnixTimeSeconds(to).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));
            await dbClient.UpdateUserLastUpdatedAsync(user.Ulid, to);

            await Task.Delay(options.RateLimitDelayInMilliseconds);
        }
    }

    private async Task HandleBeatenOrMastered(User user, Achievement achievement, Dictionary<int, GameInfoAndUserProgress> progressByGame)
    {
        //check the database to see if the user has already beaten or mastered this game
        var userGameStatus = await dbClient.GetUserGameStatusAsync(user.Ulid, achievement.GameId);
        if (userGameStatus != null && userGameStatus.Beaten && userGameStatus.Mastered) return;

        //only check this game once per polling cycle since the stats will be the same if they got multiple achievements for the same game in one cycle
        if (progressByGame.TryGetValue(achievement.GameId, out var progress)) return;

        //get user's progress in this game from RetroAchievements to see if they just beat or mastered it
        Log.Information("  RA: Getting progress for {user} in {gameTitle}...", user.Name, achievement.GameTitle);
        progress = await raClient.GetGameInfoAndUserProgressAsync(user.Ulid, achievement.GameId);
        if (progress == null)
        {
            Log.Warning("  RA: No progress found for {user} in {gameTitle}, skipping...", user.Name, achievement.GameTitle);
            return;
        }

        progressByGame[achievement.GameId] = progress;

        //determine if the user just beat or mastered the game
        var progressionAchievements = progress.Achievements.Values
            .Where(a => a.Type == "progression" || a.Type == "win_condition");
        bool beaten = progressionAchievements.All(a => a.DateEarned != null);
        bool mastered = progress.NumAchievements == progress.NumAwardedToUser;

        userGameStatus ??= new UserGameStatus
        {
            ULID = user.Ulid,
            GameID = achievement.GameId,
            NumAchievements = progress.NumAchievements,
            NumAwardedToUser = progress.NumAwardedToUser,
            Beaten = beaten,
            Mastered = mastered
        };

        if (beaten)
        {
            Log.Information("  {user} just beat {gameTitle}! ({numAwarded}/{numTotal} progression achievements)", user.Name, achievement.GameTitle, progressionAchievements.Count(a => a.DateEarned != null), progressionAchievements.Count());
            if (!mastered) //if we beat and mastered at the same time then just post the mastered message (less spammy)
            {
                foreach (var channelId in options.Discord.ChannelIds)
                {
                    Log.Information("  Discord: Posting to channel {channelId}: {user} beat {title}!", channelId, user.Name, achievement.GameTitle);
                    await discordClient.PostGameBeatenAsync(achievement, progress, user, channelId);
                    await Task.Delay(options.RateLimitDelayInMilliseconds);
                }
            }
        }
        else
        {
            Log.Information("  {user} has NOT beaten {gameTitle} yet ({numAwarded}/{numTotal} progression achievements)", user.Name, achievement.GameTitle, progressionAchievements.Count(a => a.DateEarned != null), progressionAchievements.Count());
        }

        if (mastered)
        {
            Log.Information("  {user} just mastered {gameTitle}! ({numAwarded}/{numTotal} total achievements)", user.Name, achievement.GameTitle, progress.NumAwardedToUser, progress.NumAchievements);
            foreach (var channelId in options.Discord.ChannelIds)
            {
                Log.Information("  Discord: Posting to channel {channelId}: {user} mastered {title}!", channelId, user.Name, achievement.GameTitle);
                await discordClient.PostGameMasteredAsync(achievement, progress, user, channelId);
                await Task.Delay(options.RateLimitDelayInMilliseconds);
            }
        }
        else
        {
            Log.Information("  {user} has NOT mastered {gameTitle} yet ({numAwarded}/{numTotal} total achievements)", user.Name, achievement.GameTitle, progress.NumAwardedToUser, progress.NumAchievements);
        }

        Log.Information("  DB: Saving game completion status...");
        await dbClient.SaveUserGameStatusAsync(userGameStatus);
    }
}