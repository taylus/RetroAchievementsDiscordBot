using System.Net.Http.Json;

namespace RetroAchievementsDiscordBot;

public class RetroAchievementsRestApiClient(HttpClient httpClient, string apiKey) : IRetroAchievementsClient
{
    private readonly HttpClient httpClient = httpClient;
    private readonly string apiKey = apiKey;

    public async Task<List<Achievement>> GetRecentAchievementsForUserAsync(string userId, long from, long to)
    {
        var response = await httpClient.GetAsync($"API_GetAchievementsEarnedBetween.php?y={apiKey}&u={userId}&f={from}&t={to}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Achievement>>() ?? [];
    }
}
