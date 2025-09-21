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
}