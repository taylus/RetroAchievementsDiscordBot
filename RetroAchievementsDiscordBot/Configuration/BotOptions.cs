namespace RetroAchievementsDiscordBot;

public class BotOptions
{
    public RetroAchievementsOptions RetroAchievements { get; set; } = new RetroAchievementsOptions();
    public DiscordOptions Discord { get; set; } = new DiscordOptions();
    public DatabaseOptions Database { get; set; } = new DatabaseOptions();
    public int PollingIntervalInMinutes { get; set; } = 5;
    public int RateLimitDelayInMilliseconds { get; set; } = 1000;
}
