namespace RetroAchievementsDiscordBot;

public class UserGameStatus
{
    public string ULID { get; set; } = "";
    public int GameID { get; set; }
    public int NumAchievements { get; set; }
    public int NumAwardedToUser { get; set; }
    public bool Beaten { get; set; } = false;
    public bool Mastered { get; set; } = false;
}