namespace RetroAchievementsDiscordBot;

public class UserGameStatus
{
    public required string ULID { get; set; }
    public required int GameID { get; set; }
    public required int NumAchievements { get; set; }
    public required int NumAwardedToUser { get; set; }
    public bool Beaten { get; set; } = false;
    public bool Mastered { get; set; } = false;
}