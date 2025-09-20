namespace RetroAchievementsDiscordBot;

public class GameInfoAndUserProgress
{
    public required int NumDistinctPlayers { get; set; }
    public required int NumAchievements { get; set; }
    public required int NumAwardedToUser { get; set; }
    public double UserCompletion => (double)NumAwardedToUser / NumAchievements;
    public required string ImageIcon { get; set; }
    public required Dictionary<int, ProgressAchievement> Achievements { get; set; }
    public int Points => Achievements.Values.Sum(a => a.Points);
    public int PointsAwardedToUser => Achievements.Values.Where(a => a.DateEarned != null).Sum(a => a.Points);
}

public class ProgressAchievement
{
    public required int Id { get; set; }
    public required int Points { get; set; }
    public required string Type { get; set; }
    public required int NumAwarded { get; set; }
    public string? DateEarned { get; set; }
}