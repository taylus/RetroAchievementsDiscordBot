using Microsoft.Extensions.Configuration;
using RetroAchievementsDiscordBot.Configuration;
using RetroAchievementsDiscordBot.Database;
using RetroAchievementsDiscordBot.Services;
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
            Log.Information("Starting RetroAchievements Discord bot...");
            var config = LoadConfiguration();

            var databaseClient = new DatabaseClient(config.Database.ConnectionString);
            Log.Debug("Using database: {connectionString}...", config.Database.ConnectionString);

            //var raClient = new RetroAchievementsRestApiClient(new HttpClient() { BaseAddress = new Uri(config.RetroAchievements.ApiBaseUrl) }, config.RetroAchievements.ApiKey);
            var raClient = new MockRetroAchievementsClient();

            //var discordClient = new DiscordRestApiClient(new HttpClient() { BaseAddress = new Uri(discordOptions.ApiBaseUrl) }, discordOptions.BotToken, discordOptions.ChannelIds);

            Log.Information("Polling RetroAchievements API every {interval} minutes, Ctrl+C to quit...", config.PollingIntervalInMinutes);
            while (true)
            {
                //TODO: fetch users from database and loop over each

                //get any achievements user has unlocked since we last checked
                const string username = "Taylus";
                const long from = 1758120391;   //TODO: get from database
                const long to = 1758270494;     //TODO: use now + store as last checked time later for next run
                Log.Debug("Getting achievements for {user} between {from} and {to}...", username,
                    DateTimeOffset.FromUnixTimeSeconds(from).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"),
                    DateTimeOffset.FromUnixTimeSeconds(to).ToLocalTime().ToString("yyyy-MM-dd h:mm tt"));
                var achievements = await raClient.GetRecentAchievementsForUserAsync(username, from, to);
                Log.Information("Got achievement {id}: {title}", achievements[0].AchievementId, achievements[0].Title);

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
