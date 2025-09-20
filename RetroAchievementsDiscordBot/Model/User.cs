namespace RetroAchievementsDiscordBot;

public class User
{
    public required string Ulid { get; set; }
    public required string Name { get; set; }
    public required string Avatar { get; set; }
    public required long LastUpdated { get; set; }
}