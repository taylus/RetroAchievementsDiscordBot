using Dapper;
using Microsoft.Data.Sqlite;

namespace RetroAchievementsDiscordBot;

public class DatabaseClient(string connectionString)
{
    private readonly string connectionString = connectionString;

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<User>("select ULID, Name, Avatar, LastUpdated from Users");
    }

    public async Task UpdateUserLastUpdatedAsync(string userId, long lastUpdated)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var sql = "update Users set LastUpdated = @LastUpdated where ULID = @ULID";
        await connection.ExecuteAsync(sql, new { LastUpdated = lastUpdated, ULID = userId });
    }

    public async Task<bool> AchievementUnlockExistsAsync(string userId, int achievementId)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var sql = "select 1 from UserAchievements where ULID = @ULID AND AchievementId = @AchievementId limit 1";
        var result = await connection.ExecuteScalarAsync<int?>(sql, new { ULID = userId, AchievementId = achievementId });
        return result.HasValue;
    }

    public async Task SaveAchievementUnlockAsync(string userId, Achievement achievement)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("insert into UserAchievements (ULID, AchievementID, " +
            "UnlockedAt, Title, Description, Points, GameTitle, GameID, ConsoleName, BadgeUrl) " +
            "values (@ULID, @AchievementId, @UnlockedAt, @Title, @Description, @Points, @GameTitle, @GameId, @ConsoleName, @BadgeUrl)", new
            {
                ULID = userId,
                achievement.AchievementId,
                UnlockedAt = achievement.DateTimeOffset.ToUnixTimeSeconds(),
                achievement.Title,
                achievement.Description,
                achievement.Points,
                achievement.GameTitle,
                achievement.GameId,
                achievement.ConsoleName,
                achievement.BadgeUrl
            });
    }
}
