using RetroAchievementsDiscordBot.Model;

namespace RetroAchievementsDiscordBot.Services;

public interface IRetroAchievementsClient
{
    Task<List<Achievement>> GetRecentAchievementsForUserAsync(string userId, long from, long to);
}