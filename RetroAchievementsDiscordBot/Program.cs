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
            var bot = ConfigureBot(config);

            Log.Information("Polling RetroAchievements API every {interval} minutes, Ctrl+C to quit", config.PollingIntervalInMinutes);
            while (true)
            {
                Log.Debug("Beginning polling cycle:");
                await bot.PollForAchievementsAndPostToDiscord();

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

    private static Bot ConfigureBot(BotOptions config)
    {
        Log.Debug("Using database: {connectionString}", config.Database.ConnectionString);
        var databaseClient = new DatabaseClient(config.Database.ConnectionString);

        Log.Debug("Using RetroAchievements API: {apiUrl}", config.RetroAchievements.ApiBaseUrl);
        var raClient = new RetroAchievementsRestApiClient(new HttpClient() { BaseAddress = new Uri(config.RetroAchievements.ApiBaseUrl) }, config.RetroAchievements.ApiKey);
        //var raClient = new MockRetroAchievementsClient();

        Log.Debug("Using Discord API: {apiUrl}", config.Discord.ApiBaseUrl);
        var discordClient = new DiscordRestApiClient(new HttpClient() { BaseAddress = new Uri(config.Discord.ApiBaseUrl) }, config.Discord.BotToken);

        return new Bot(config, databaseClient, raClient, discordClient);
    }
}
