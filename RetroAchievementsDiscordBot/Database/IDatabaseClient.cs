namespace RetroAchievementsDiscordBot;

public interface IDatabaseClient
{
    Task<bool> AchievementUnlockExistsAsync(string userId, int achievementId);
    Task<IEnumerable<User>> GetUsersAsync();
    Task SaveAchievementUnlockAsync(string userId, Achievement achievement);
    Task UpdateUserLastUpdatedAsync(string userId, long lastUpdated);
}