namespace RetroAchievementsDiscordBot;

public interface IDiscordRestApiClient
{
    Task PostAchievementUnlockAsync(Achievement achievement, User unlockedBy, string channelId);
    Task PostGameBeatenAsync(Achievement achievement, GameInfoAndUserProgress progress, User user, string channelId);
    Task PostGameMasteredAsync(Achievement achievement, GameInfoAndUserProgress progress, User user, string channelId);
}