namespace RetroAchievementsDiscordBot;

public interface IDatabaseClient
{
    Task<bool> AchievementUnlockExistsAsync(string userId, int achievementId);
    Task<UserGameStatus?> GetUserGameStatusAsync(string userId, int gameId);
    Task<IEnumerable<User>> GetUsersAsync();
    Task SaveAchievementUnlockAsync(string userId, Achievement achievement);
    Task SaveUserGameStatusAsync(UserGameStatus userGameStatus);
    Task UpdateUserLastUpdatedAsync(string userId, long lastUpdated);
}