using Moq;
using Serilog;

namespace RetroAchievementsDiscordBot.Tests;

[TestClass]
public class Achievement_Unlocks_Should
{
    [TestInitialize]
    public void Setup()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Log.CloseAndFlush();
    }

    [TestMethod]
    public async Task Post_Unlocked_Message_To_Discord()
    {
        //arrange 1 user with 1 new achievement unlock
        var dbMock = new Mock<IDatabaseClient>();
        dbMock.Setup(d => d.GetUsersAsync()).ReturnsAsync([new User() { Name = "TestUser" }]);
        var raMock = new Mock<IRetroAchievementsClient>();
        raMock.Setup(r => r.GetRecentAchievementsForUserAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync([
            new Achievement() { Title = "Test Achievement", GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() }]);
        var discordMock = new Mock<IDiscordRestApiClient>();
        var bot = new Bot(TestOptions, dbMock.Object, raMock.Object, discordMock.Object);

        //act
        await bot.PollForAchievementsAndPostToDiscord();

        //assert unlock message is posted
        discordMock.Verify(d => d.PostAchievementUnlockAsync(
            It.IsAny<Achievement>(), It.IsAny<User>(), It.IsAny<string>()), Times.Once,
            "Achievement unlock should have been posted to Discord.");

        //assert beaten message is NOT posted
        discordMock.Verify(d => d.PostGameBeatenAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game beaten should NOT have been posted to Discord.");

        //assert mastered message is NOT posted
        discordMock.Verify(d => d.PostGameMasteredAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game mastered should NOT have been posted to Discord.");
    }

    [TestMethod]
    public async Task Not_Post_To_Discord_If_Already_Sent()
    {
        //arrange 1 user with 1 achievement unlock that has already been posted
        var dbMock = new Mock<IDatabaseClient>();
        dbMock.Setup(d => d.GetUsersAsync()).ReturnsAsync([new User() { Name = "TestUser" }]);
        dbMock.Setup(d => d.AchievementUnlockExistsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);
        var raMock = new Mock<IRetroAchievementsClient>();
        raMock.Setup(r => r.GetRecentAchievementsForUserAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync([
            new Achievement() { Title = "Test Achievement", GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() }]);
        var discordMock = new Mock<IDiscordRestApiClient>();
        var bot = new Bot(TestOptions, dbMock.Object, raMock.Object, discordMock.Object);

        //act
        await bot.PollForAchievementsAndPostToDiscord();

        //assert unlock message is NOT posted
        discordMock.Verify(d => d.PostAchievementUnlockAsync(
            It.IsAny<Achievement>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Achievement unlock should NOT have been posted to Discord.");

        //assert beaten message is NOT posted
        discordMock.Verify(d => d.PostGameBeatenAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game beaten should NOT have been posted to Discord.");

        //assert mastered message is NOT posted
        discordMock.Verify(d => d.PostGameMasteredAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game mastered should NOT have been posted to Discord.");
    }

    [TestMethod]
    public async Task Post_Beaten_Message_To_Discord()
    {
        //arrange multiple unlocks for the same game that beat it
        var dbMock = new Mock<IDatabaseClient>();
        dbMock.Setup(d => d.GetUsersAsync()).ReturnsAsync([new User() { Name = "TestUser" }]);
        dbMock.SetupSequence(d => d.GetUserGameStatusAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((UserGameStatus?)null) //first call (cycle #1 achievement #1), no status yet
            .ReturnsAsync(new UserGameStatus() { Beaten = true }) //second call (cycle #1 achievement #2), now beaten
            .ReturnsAsync(new UserGameStatus() { Beaten = true }) //subsequent calls, still beaten
            .ReturnsAsync(new UserGameStatus() { Beaten = true }); //subsequent calls, still beaten
        var raMock = new Mock<IRetroAchievementsClient>();
        raMock.Setup(r => r.GetRecentAchievementsForUserAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync([
            new Achievement() { Title = "Test Achievement #1", GameId = 100, GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() },
            new Achievement() { Title = "Test Achievement #2", GameId = 100, GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() }]);
        raMock.Setup(r => r.GetGameInfoAndUserProgressAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(
            new GameInfoAndUserProgress()
            {
                NumAchievements = 2,
                NumAwardedToUser = 1,
                Achievements = {
                    [1] = new ProgressAchievement() { Type = "win_condition", DateEarned = DateTime.UtcNow.ToString() },
                    [2] = new ProgressAchievement() { Type = "" }
                },
            }
        );
        var discordMock = new Mock<IDiscordRestApiClient>();
        var bot = new Bot(TestOptions, dbMock.Object, raMock.Object, discordMock.Object);

        //act
        await bot.PollForAchievementsAndPostToDiscord();
        await bot.PollForAchievementsAndPostToDiscord(); //run twice to be sure it only posts once

        //assert beaten message is posted once
        discordMock.Verify(d => d.PostGameBeatenAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Once,
            "Game beaten should have been posted to Discord.");

        //assert mastered message is NOT posted
        discordMock.Verify(d => d.PostGameMasteredAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game mastered should NOT have been posted to Discord.");
    }

    [TestMethod]
    public async Task Post_Mastered_Message_To_Discord()
    {
        //arrange multiple unlocks for the same game that mastered it
        var dbMock = new Mock<IDatabaseClient>();
        dbMock.Setup(d => d.GetUsersAsync()).ReturnsAsync([new User() { Name = "TestUser" }]);
        dbMock.SetupSequence(d => d.GetUserGameStatusAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((UserGameStatus?)null) //first call (cycle #1 achievement #1), no status yet
            .ReturnsAsync(new UserGameStatus() { Beaten = true, Mastered = true }) //second call (cycle #1 achievement #2), now mastered
            .ReturnsAsync(new UserGameStatus() { Beaten = true, Mastered = true }) //subsequent calls, still mastered
            .ReturnsAsync(new UserGameStatus() { Beaten = true, Mastered = true }); //subsequent calls, still mastered
        var raMock = new Mock<IRetroAchievementsClient>();
        raMock.Setup(r => r.GetRecentAchievementsForUserAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync([
            new Achievement() { Title = "Test Achievement #1", GameId = 100, GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() },
            new Achievement() { Title = "Test Achievement #2", GameId = 100, GameTitle = "Test Game", Date = DateTime.UtcNow.ToString() }]);
        raMock.Setup(r => r.GetGameInfoAndUserProgressAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(
            new GameInfoAndUserProgress()
            {
                NumAchievements = 2,
                NumAwardedToUser = 2,
                Achievements = {
                    [1] = new ProgressAchievement() { Type = "progression", DateEarned = DateTime.UtcNow.ToString() },
                    [2] = new ProgressAchievement() { Type = "win_condition", DateEarned = DateTime.UtcNow.ToString() }
                },
            }
        );
        var discordMock = new Mock<IDiscordRestApiClient>();
        var bot = new Bot(TestOptions, dbMock.Object, raMock.Object, discordMock.Object);

        //act
        await bot.PollForAchievementsAndPostToDiscord();
        await bot.PollForAchievementsAndPostToDiscord(); //run twice to be sure it only posts once

        //assert mastered message is posted once
        discordMock.Verify(d => d.PostGameMasteredAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Once,
            "Game mastered should have been posted to Discord.");

        //assert beaten message is NOT posted
        discordMock.Verify(d => d.PostGameBeatenAsync(
            It.IsAny<Achievement>(), It.IsAny<GameInfoAndUserProgress>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never,
            "Game beaten should NOT have been posted to Discord.");
    }

    private static BotOptions TestOptions => new()
    {
        RateLimitDelayInMilliseconds = 0,
        Discord = new DiscordOptions
        {
            ChannelIds = ["test-channel-id"]
        },
    };
}