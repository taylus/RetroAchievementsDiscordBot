using System.Globalization;

namespace RetroAchievementsDiscordBot;

public class Achievement
{
    public int AchievementId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int Points { get; set; }
    public string GameTitle { get; set; } = "";
    public int GameId { get; set; }
    public string ConsoleName { get; set; } = "";
    public string BadgeUrl { get; set; } = "";
    public string Date { get; set; } = "";
    public DateTimeOffset DateTimeOffset => DateTimeOffset.TryParse(Date, CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date) ? date : DateTimeOffset.MinValue;
}
