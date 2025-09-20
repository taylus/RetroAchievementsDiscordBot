namespace RetroAchievementsDiscordBot.Services;

public class DiscordRestApiClient(HttpClient httpClient, string botToken, string[] channelIds)
{
    private readonly HttpClient httpClient = httpClient;
    private readonly string botToken = botToken;
    private readonly string[] channelIds = channelIds;

    //TODO: implement sending messages to Discord channels via REST API
}
