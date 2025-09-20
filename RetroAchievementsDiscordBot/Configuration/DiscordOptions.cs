namespace RetroAchievementsDiscordBot;

public class DiscordOptions
{
    public required string ApiBaseUrl { get; set; }
    public required string BotToken { get; set; }
    public required string[] ChannelIds { get; set; }
}