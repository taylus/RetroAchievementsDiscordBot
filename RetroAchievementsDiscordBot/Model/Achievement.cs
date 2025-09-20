using System.Globalization;

namespace RetroAchievementsDiscordBot;

public class Achievement
{
    public required int AchievementId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int Points { get; set; }
    public required string GameTitle { get; set; }
    public required int GameId { get; set; }
    public required string ConsoleName { get; set; }
    public required string BadgeUrl { get; set; }
    public required string Date { get; set; }
    public DateTimeOffset DateTimeOffset => DateTimeOffset.Parse(Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
