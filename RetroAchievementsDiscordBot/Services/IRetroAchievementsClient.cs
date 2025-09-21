namespace RetroAchievementsDiscordBot;

public interface IRetroAchievementsClient
{
    Task<List<Achievement>> GetRecentAchievementsForUserAsync(string userId, long from, long to);
}