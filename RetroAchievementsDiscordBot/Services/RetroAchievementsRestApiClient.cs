using System.Net.Http.Json;
using Serilog;

namespace RetroAchievementsDiscordBot;

public class RetroAchievementsRestApiClient(HttpClient httpClient, string apiKey) : IRetroAchievementsClient
{
    private readonly HttpClient httpClient = httpClient;
    private readonly string apiKey = apiKey;

    public async Task<List<Achievement>> GetRecentAchievementsForUserAsync(string userId, long from, long to)
    {
        var response = await httpClient.GetAsync($"API_GetAchievementsEarnedBetween.php?y={apiKey}&u={userId}&f={from}&t={to}");
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Log.Error("  RA: Failed to get achievements: {statusCode} - {responseContent}", response.StatusCode, responseContent);
            return [];
        }
        var achievements = await response.Content.ReadFromJsonAsync<List<Achievement>>() ?? [];
        Log.Information("  RA: Got {count} achievement(s)", achievements.Count);
        return achievements;
    }
}
