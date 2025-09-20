using RetroAchievementsDiscordBot.Model;

namespace RetroAchievementsDiscordBot.Services;

public class MockRetroAchievementsClient() : IRetroAchievementsClient
{
    public async Task<List<Achievement>> GetRecentAchievementsForUserAsync(string userId, long from, long to)
    {
        var achievements = new List<Achievement>()
        {
            new()
            {
                AchievementId = 12345,
                Title = "Test Achievement",
                Description = "This is a fake achievement for testing.",
                Points = 10,
                GameTitle = "Test Game",
                GameId = 1,
                ConsoleName = "Test Console",
                BadgeUrl = "https://static.retroachievements.org/assets/images/ra-icon.webp"
            }
        };

        return await Task.FromResult(achievements);
    }
}
