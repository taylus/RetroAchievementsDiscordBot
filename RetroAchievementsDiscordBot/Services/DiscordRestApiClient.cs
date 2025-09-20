using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Serilog;

namespace RetroAchievementsDiscordBot;

public class DiscordRestApiClient(HttpClient httpClient, string botToken)
{
    private readonly HttpClient httpClient = httpClient;
    private readonly string botToken = botToken;

    public async Task PostAchievementUnlockToChannel(Achievement achievement, User unlockedBy, string channelId)
    {
        var requestBody = CreateRequestBody(achievement, unlockedBy);
        var request = CreateRequest(HttpMethod.Post, $"channels/{channelId}/messages", botToken, requestBody);
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            Log.Information("  Discord: Posted to channel {channelId}: {user} unlocked {title}!", channelId, unlockedBy.Name, achievement.Title);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Log.Error("  Discord: Failed to post achievement unlock to channel {channelId}: {statusCode} - {responseContent}", channelId, response.StatusCode, responseContent);
        }

        static HttpRequestMessage CreateRequest(HttpMethod method, string uri, string botToken, StringContent content)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bot", botToken);
            request.Content = content;
            return request;
        }

        static StringContent CreateRequestBody(Achievement achievement, User unlockedBy)
        {
            var payload = new
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
                        timestamp = achievement.Date.ToString("o")
                    }
                }
            };
            string json = JsonSerializer.Serialize(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
