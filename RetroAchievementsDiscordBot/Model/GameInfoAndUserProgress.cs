namespace RetroAchievementsDiscordBot;

public class GameInfoAndUserProgress
{
    public int NumDistinctPlayers { get; set; }
    public int NumAchievements { get; set; }
    public int NumAwardedToUser { get; set; }
    public double UserCompletion => (double)NumAwardedToUser / NumAchievements;
    public string ImageIcon { get; set; } = "";
    public Dictionary<int, ProgressAchievement> Achievements { get; set; } = new Dictionary<int, ProgressAchievement>();
    public int Points => Achievements.Values.Sum(a => a.Points);
    public int PointsAwardedToUser => Achievements.Values.Where(a => a.DateEarned != null).Sum(a => a.Points);
}

public class ProgressAchievement
{
    public int Id { get; set; }
    public int Points { get; set; }
    public string Type { get; set; } = "";
    public int NumAwarded { get; set; }
    public string? DateEarned { get; set; }
}
