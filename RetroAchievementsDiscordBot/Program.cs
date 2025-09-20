using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace RetroAchievementsDiscordBot;

public class Program
{
    public static async Task Main()
    {
        Log.Logger = ConfigureLogging();
        try
        {
            Log.Information("{banner} RetroAchievements Discord Bot {banner}", new string('=', 30), new string('=', 30));
            var config = LoadConfiguration();

            var databaseClient = new DatabaseClient(config.Database.ConnectionString);
            Log.Debug("Using database: {connectionString}", config.Database.ConnectionString);
            Log.Debug("Using RetroAchievements API: {apiUrl}", config.RetroAchievements.ApiBaseUrl);
            Log.Debug("Using Discord API: {apiUrl}", config.Discord.ApiBaseUrl);

            var raClient = new RetroAchievementsRestApiClient(new HttpClient() { BaseAddress = new Uri(config.RetroAchievements.ApiBaseUrl) }, config.RetroAchievements.ApiKey);
            //var raClient = new MockRetroAchievementsClient();

            var discordClient = new DiscordRestApiClient(new HttpClient() { BaseAddress = new Uri(config.Discord.ApiBaseUrl) }, config.Discord.BotToken);

            Log.Information("Polling RetroAchievements API every {interval} minutes, Ctrl+C to quit", config.PollingIntervalInMinutes);
            while (true)
            {
                Log.Debug("Beginning polling cycle:");
                var users = await databaseClient.GetUsersAsync();
                Log.Debug("  DB: Got {count} user(s) from database", users.Count());
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
                        Log.Information("  RA: {user} unlocked {title} for {gameTitle} on {consoleName}", user.Name, achievement.Title, achievement.GameTitle, achievement.ConsoleName);
                        
                        //check the database to see if we've already posted this
                        if (await databaseClient.AchievementUnlockExistsAsync(user.Ulid, achievement.AchievementId))
                        {
                            Log.Debug("  DB: Achievement {achievementId} for {user} already exists in database, skipping...", achievement.AchievementId, user.Name);
                            continue;
                        }

                        foreach (var channelId in config.Discord.ChannelIds)
                        {
                            Log.Information("  Discord: Posting to channel {channelId}: {user} unlocked {title}!", channelId, user.Name, achievement.Title);
                            await discordClient.PostAchievementUnlockToChannelAsync(achievement, user, channelId);
                            await Task.Delay(1000); //avoid Discord rate limits
                        }

                        //save this unlock to the database so we don't post it again
                        Log.Debug("  DB: Saving achievement {achievementId} for {user} to database...", achievement.AchievementId, user.Name);
                        await databaseClient.SaveAchievementUnlockAsync(user.Ulid, achievement);
                    }

                    //update the user's last updated time so next run we only get new achievements
                    Log.Debug("  DB: Setting LastUpdated for {user} to {to}...", user.Name, DateTimeOffset.FromUnixTimeSeconds(to).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));
                    await databaseClient.UpdateUserLastUpdatedAsync(user.Ulid, to);

                    await Task.Delay(1000); //avoid RA rate limits
                }

                Log.Debug("Waiting {interval} minutes for next polling cycle...", config.PollingIntervalInMinutes);
                await Task.Delay(TimeSpan.FromMinutes(config.PollingIntervalInMinutes));
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static Logger ConfigureLogging() => new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/ra-bot-.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    private static BotOptions LoadConfiguration()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        return config.Get<BotOptions>() ?? throw new Exception("Failed to load configuration.");
    }
}
