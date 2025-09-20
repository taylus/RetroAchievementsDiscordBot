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

            //var raClient = new RetroAchievementsRestApiClient(new HttpClient() { BaseAddress = new Uri(config.RetroAchievements.ApiBaseUrl) }, config.RetroAchievements.ApiKey);
            var raClient = new MockRetroAchievementsClient();

            var discordClient = new DiscordRestApiClient(new HttpClient() { BaseAddress = new Uri(config.Discord.ApiBaseUrl) }, config.Discord.BotToken);

            Log.Information("Polling RetroAchievements API every {interval} minutes, Ctrl+C to quit", config.PollingIntervalInMinutes);
            while (true)
            {
                Log.Debug("Beginning polling cycle:");

                //TODO: fetch users from database and loop over each
                var user = new User() { Ulid = "", Name = "Taylus", Avatar = "/UserPic/Taylus.png", LastUpdated = 1758120391 };

                //get any achievements user has unlocked since we last checked
                const long to = 1758270494;     //TODO: use now + store as last checked time later for next run
                Log.Debug("  RA: Getting achievements for {user} between [{from}] and [{to}]...", user.Name,
                    DateTimeOffset.FromUnixTimeSeconds(user.LastUpdated).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"),
                    DateTimeOffset.FromUnixTimeSeconds(to).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));
                var achievements = await raClient.GetRecentAchievementsForUserAsync(user.Name, user.LastUpdated, to);
                Log.Information("  RA: Got {count} achievement(s): {list}", achievements.Count, achievements.Select(a => a.AchievementId));

                //post any new achievements to Discord
                foreach (var achievement in achievements)
                {
                    foreach (var channelId in config.Discord.ChannelIds)
                    {
                        Log.Information("  Discord: Posting achievement {id} to channel {channelId}... ", achievement.AchievementId, channelId);
                        await discordClient.PostAchievementUnlockToChannel(achievement, user, channelId);
                    }
                }

                Log.Debug("Waiting {interval} minutes...", config.PollingIntervalInMinutes);
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
