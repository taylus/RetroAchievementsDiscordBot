namespace RetroAchievementsDiscordBot;

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
                BadgeUrl = "/assets/images/ra-icon.webp",
                Date = DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return await Task.FromResult(achievements);
    }

    public async Task<GameInfoAndUserProgress?> GetGameInfoAndUserProgressAsync(string userId, int gameId)
    {
        var progress = new GameInfoAndUserProgress()
        {
            NumDistinctPlayers = 100,
            NumAchievements = 2,
            NumAwardedToUser = 1,
            ImageIcon = "/assets/images/ra-icon.webp",
            Achievements = new Dictionary<int, ProgressAchievement>
            {
                [12345] = new ProgressAchievement
                {
                    Id = 12345,
                    Points = 10,
                    Type = "win_condition",
                    NumAwarded = 40,
                    DateEarned = DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")
                },
                [23456] = new ProgressAchievement
                {
                    Id = 23456,
                    Points = 10,
                    Type = "",
                    NumAwarded = 15,
                    DateEarned = null
                }
            }
        };

        return await Task.FromResult(progress);
    }
}
