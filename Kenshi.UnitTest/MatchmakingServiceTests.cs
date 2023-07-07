using Kenshi.API;
using Kenshi.API.Hub;
using Kenshi.API.Models;
using Kenshi.API.Models.Concrete;
using Kenshi.API.Models.Enums;
using Kenshi.API.Services;
using Kenshi.API.Services.Concrete;
using Kenshi.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Kenshi.UnitTest;

[TestFixture]
public class MatchmakingServiceTests
{
    private Mock<ILogger<MatchmakingService>> _logger;
    private Mock<IGameRoomService> _mockGameRoomService;
    private Mock<IHubContext<GameHub>> _mockGameHubContext;
    private Mock<IHubClients> _mockHubClients;
    private Mock<IClientProxy> _mockClientProxy;
    private MatchmakingService _matchmakingService;
    private GameRoomInstance _roomInstance;
    private int id = 5000;

    [SetUp]
    protected void Setup()
    {
        _logger = new Mock<ILogger<MatchmakingService>>();
        _mockGameRoomService = new Mock<IGameRoomService>();
        _mockGameHubContext = new Mock<IHubContext<GameHub>>();
        _mockHubClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _matchmakingService = new MatchmakingService(_logger.Object, _mockGameRoomService.Object, _mockGameHubContext.Object);

        _mockHubClients.Setup(h => h.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockGameHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);

        _mockGameRoomService.Setup(m => m.CreateRoom(It.IsAny<string>(), It.IsAny<bool>())).Returns(()=> _roomInstance = new GameRoomInstance()
        {
            Port = id++,
            DisplayName = "Test-Room",
            RoomNumber = "1",
            Started = false,
            LeaderUsername = "test",
            Settings = new RoomSettingsDto()
        });
        
        _mockGameRoomService.Setup(m => m.StartGameInstance(_roomInstance));
    }
    
    [Test]
    [TestCase(3, 4, 0)]
    [TestCase(4, 4, 1)]
    [TestCase(6, 4, 1)]
    [TestCase(10, 4, 2)]
    [TestCase(12, 5, 2)]
    public async Task UpdateMatchmakingLobbies_WhenCalled_MatchesRoomNumber(int numPlayers, int roomSize, int expectedNumGameRooms)
    {
        // Arrange
        var lobbies = PopulateLobbies(numPlayers);

        // Act
        await _matchmakingService.UpdateMatchmakingLobbies(lobbies.ToHashSet(), roomSize);
        
        // Assert
        _mockGameRoomService.Verify(m => m.CreateRoom("room", false), Times.Exactly(expectedNumGameRooms));
    }
    
    [Test]
    public async Task UpdateMatchmakingLobbies_WhenCalled_MatchesRoomUsername()
    {
        // Arrange
        var lobby1 = new Lobby()
        {
            Id = "1",
            Users = new List<GameUserService.GameUser>()
            {
                new GameUserService.GameUser() { Username = "User1" },
                new GameUserService.GameUser() { Username = "User2" },
                new GameUserService.GameUser() { Username = "User3" },
                new GameUserService.GameUser() { Username = "User4" },
            },
            State = MatchmakingState.Searching,
            WaitStartTime = DateTimeOffset.Now
        };

        foreach (var user in lobby1.Users)
        {
            user.Lobby = lobby1;
        }
        var lobby2 = new Lobby()
        {
            Id = "2",
            Users = new List<GameUserService.GameUser>()
            {
                new GameUserService.GameUser() { Username = "User5" },
                new GameUserService.GameUser() { Username = "User6" },
                new GameUserService.GameUser() { Username = "User7" },
                new GameUserService.GameUser() { Username = "User8" },
            },
            State = MatchmakingState.Searching,
            WaitStartTime = DateTimeOffset.Now
        };
        foreach (var user in lobby2.Users)
        {
            user.Lobby = lobby2;
        }
        var lobby3 = new Lobby()
        {
            Id = "3",
            Users = new List<GameUserService.GameUser>()
            {
                new GameUserService.GameUser() { Username = "User9" },
                new GameUserService.GameUser() { Username = "User10" },
                new GameUserService.GameUser() { Username = "User11" },
            },
            State = MatchmakingState.Searching,
            WaitStartTime = DateTimeOffset.Now
        };
        foreach (var user in lobby3.Users)
        {
            user.Lobby = lobby3;
        }
        
        // Act
        var rooms = await _matchmakingService.UpdateMatchmakingLobbies(new HashSet<Lobby>(){lobby1, lobby2, lobby3}.ToHashSet(), 4);
        
        //Assert
        Assert.IsTrue(rooms.Count() == 2);
        Assert.IsTrue(rooms.All(r => r.Players.Count == 4));
    }
    
    [Test]
    [TestCase(4, 10, 0)]
    [TestCase(4, 30, 1)]
    public async Task UpdateMatchmakingLobbies_WhenCalled_MatchesRoomNumber_WhenSoloPlayer(int roomSize, float waitTimeInSeconds, int expectedNumGameRooms)
    {
        // Arrange
        Lobby lastLobby = new Lobby()
        {
            Id = "1",
            Users = new List<GameUserService.GameUser>(),
            State = MatchmakingState.Searching,
            WaitStartTime = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(waitTimeInSeconds))
        };
        
        lastLobby.Users.Add(new GameUserService.GameUser()
        {
            Lobby = lastLobby,
            Id = "1",
            ConnectionId = "con1",
            Username = "user",
        });

        // Act
        var rooms = await _matchmakingService.UpdateMatchmakingLobbies(new HashSet<Lobby>(){ lastLobby }, roomSize);
        
        // Assert
        _mockGameRoomService.Verify(m => m.CreateRoom("room", false), Times.Exactly(expectedNumGameRooms));
    }

    private static IEnumerable<Lobby> PopulateLobbies(int numPlayers)
    {
        var lobbies = new List<Lobby>();
        Lobby lastLobby = null;
        for (int i = 0; i < numPlayers; i++)
        {
            if (i % Constants.LOBBY_SIZE == 0)
            {
                lastLobby = new Lobby()
                {
                    Id = lobbies.Count.ToString(),
                    State = MatchmakingState.Searching,
                    WaitStartTime = DateTimeOffset.Now
                };
                lobbies.Add(lastLobby);
            }

            var user = new GameUserService.GameUser
                { Username = $"user{i}", ConnectionId = $"connection{i}", Lobby = lastLobby };
            
            lastLobby.Users.Add(user);
        }

        return lobbies;
    }
}