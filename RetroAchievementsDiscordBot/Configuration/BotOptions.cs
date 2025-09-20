namespace RetroAchievementsDiscordBot;

public class BotOptions
{
    public required RetroAchievementsOptions RetroAchievements { get; set; }
    public required DiscordOptions Discord { get; set; }
    public required DatabaseOptions Database { get; set; }
    public required int PollingIntervalInMinutes { get; set; } = 5;
}
