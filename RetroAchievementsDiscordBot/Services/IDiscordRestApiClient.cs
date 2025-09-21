namespace RetroAchievementsDiscordBot;

public interface IDiscordRestApiClient
{
    Task PostAchievementUnlockAsync(Achievement achievement, User unlockedBy, string channelId);
}