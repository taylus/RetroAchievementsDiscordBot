using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Serilog;

namespace RetroAchievementsDiscordBot;

public class DiscordRestApiClient(HttpClient httpClient, string botToken)
{
    private readonly HttpClient httpClient = httpClient;
    private readonly string botToken = botToken;

    public async Task PostAchievementUnlockToChannelAsync(Achievement achievement, User unlockedBy, string channelId)
    {
        var requestBody = CreateRequestBody(achievement, unlockedBy);
        var request = CreateRequest(HttpMethod.Post, $"channels/{channelId}/messages", botToken, requestBody);
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            Log.Information("  Discord: Posted to channel {channelId} successfully", channelId);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Log.Error("  Discord: Failed to post achievement unlock to channel {channelId}: {statusCode} - {responseContent}", channelId, response.StatusCode, responseContent);
        }

        static StringContent CreateRequestBody(Achievement achievement, User unlockedBy)
        {
            return new StringContent(JsonSerializer.Serialize(new
            {
                embeds = new[]
                {
                    new
                    {
                        color = 1140445,
                        author = new
                        {
                            name = achievement.GameTitle,
                            url = $"https://retroachievements.org/game/{achievement.GameId}",
                            //TODO: icon_url = console icon
                        },
                        title = $"{unlockedBy.Name} unlocked {achievement.Title}!",
                        url = $"https://retroachievements.org/achievement/{achievement.AchievementId}",
                        description = achievement.Description,
                        thumbnail = new { url = "https://retroachievements.org" + achievement.BadgeUrl },
                        fields = new[]
                        {
                            new { name = "Points", value = achievement.Points.ToString(), inline = true },
                            new { name = "Console", value = achievement.ConsoleName, inline = true },
                        },
                        footer = new
                        {
                            text = $"Unlocked by {unlockedBy.Name}",
                            icon_url = "https://retroachievements.org" + unlockedBy.Avatar
                        },
                        timestamp = achievement.DateTimeOffset.ToString("o")
                    }
                }
            }), Encoding.UTF8, "application/json");
        }
    }

    public async Task PostGameBeatenToChannelAsync(Achievement achievement, GameInfoAndUserProgress progress, User user, string channelId)
    {
        var requestBody = CreateRequestBody(achievement, progress, user);
        var request = CreateRequest(HttpMethod.Post, $"channels/{channelId}/messages", botToken, requestBody);
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            Log.Information("  Discord: Posted to channel {channelId} successfully", channelId);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Log.Error("  Discord: Failed to post game beaten to channel {channelId}: {statusCode} - {responseContent}", channelId, response.StatusCode, responseContent);
        }

        static StringContent CreateRequestBody(Achievement achievement, GameInfoAndUserProgress progress, User user)
        {
            return new StringContent(JsonSerializer.Serialize(new
            {
                embeds = new[]
                {
                    new
                    {
                        color = 15381000,
                        author = new
                        {
                            name = achievement.GameTitle,
                            url = $"https://retroachievements.org/game/{achievement.GameId}",
                            //TODO: icon_url = beaten icon
                        },
                        title = $"{user.Name} beat {achievement.GameTitle}!",
                        url = $"https://retroachievements.org/game/{achievement.GameId}",
                        thumbnail = new { url = "https://retroachievements.org" + progress.ImageIcon },
                        fields = new[]
                        {
                            new { name = "Achievements", value = $"{progress.NumAwardedToUser} of {progress.NumAchievements}", inline = true },
                            new { name = "Points", value = $"{progress.PointsAwardedToUser} of {progress.Points}", inline = true },
                            new { name = "Console", value = achievement.ConsoleName, inline = true },
                        },
                        footer = new
                        {
                            text = $"Beaten by {user.Name}",
                            icon_url = "https://retroachievements.org" + user.Avatar
                        },
                        timestamp = achievement.DateTimeOffset.ToString("o")
                    }
                }
            }), Encoding.UTF8, "application/json");
        }
    }

    public async Task PostGameMasteredToChannelAsync(Achievement achievement, GameInfoAndUserProgress progress, User user, string channelId)
    {
        var requestBody = CreateRequestBody(achievement, progress, user);
        var request = CreateRequest(HttpMethod.Post, $"channels/{channelId}/messages", botToken, requestBody);
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            Log.Information("  Discord: Posted to channel {channelId} successfully", channelId);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Log.Error("  Discord: Failed to post game beaten to channel {channelId}: {statusCode} - {responseContent}", channelId, response.StatusCode, responseContent);
        }

        static StringContent CreateRequestBody(Achievement achievement, GameInfoAndUserProgress progress, User user)
        {
            return new StringContent(JsonSerializer.Serialize(new
            {
                embeds = new[]
                {
                    new
                    {
                        color = 15381000,
                        author = new
                        {
                            name = achievement.GameTitle,
                            url = $"https://retroachievements.org/game/{achievement.GameId}",
                            //TODO: icon_url = mastered icon
                        },
                        title = $"{user.Name} mastered {achievement.GameTitle}!",
                        url = $"https://retroachievements.org/game/{achievement.GameId}",
                        thumbnail = new { url = "https://retroachievements.org" + progress.ImageIcon },
                        fields = new[]
                        {
                            new { name = "Achievements", value = $"{progress.NumAwardedToUser} of {progress.NumAchievements}", inline = true },
                            new { name = "Points", value = $"{progress.PointsAwardedToUser} of {progress.Points}", inline = true },
                            new { name = "Console", value = achievement.ConsoleName, inline = true },
                        },
                        footer = new
                        {
                            text = $"Mastered by {user.Name}",
                            icon_url = "https://retroachievements.org" + user.Avatar
                        },
                        timestamp = achievement.DateTimeOffset.ToString("o")
                    }
                }
            }), Encoding.UTF8, "application/json");
        }
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, string botToken, StringContent content)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", botToken);
        request.Content = content;
        return request;
    }
}
