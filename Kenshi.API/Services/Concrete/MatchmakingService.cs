using Kenshi.API.Hub;
using Kenshi.API.Models;
using Kenshi.API.Models.Enums;
using Kenshi.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Enum = Google.Protobuf.WellKnownTypes.Enum;

namespace Kenshi.API.Services.Concrete;

public class MatchmakingService : IMatchmakingService
{
    public static HashSet<Lobby> Lobbies { get; set; }

    private readonly ILogger<MatchmakingService> _logger;
    private readonly IGameRoomService _gameRoomService;
    private readonly IHubContext<GameHub> _gameHubContext;

    private const float MAX_SOLO_PLAYER_WAIT_TIME = 20;
    private const int MIN_PLAYERS_AMOUNT_TO_START_GAMEROOM = 2;
    public const int PLAYERS_ROOM_SIZE_TO_TAKE_FOR_GAME = 2;

    public MatchmakingService(
        ILogger<MatchmakingService> logger,
        IGameRoomService gameRoomService,
        IHubContext<GameHub> gameHubContext)
    {
        _logger = logger;
        _gameRoomService = gameRoomService;
        _gameHubContext = gameHubContext;

        if (Lobbies is null)
        {
            Lobbies = new HashSet<Lobby>();
        }
    }
    
    private static void PopulateMergedLobby(Lobby targetLobby, List<Lobby> lobbies, List<Lobby> matchedLobbies, int roomSize)
    {
        while (targetLobby.Users.Count < roomSize / 2)
        {
            var lobbiesLeft = lobbies.ToList();

            if (lobbiesLeft.Count == 0)
                break;

            foreach (var lobby in lobbiesLeft.ToList())
            {
                if (targetLobby.Users.Count == roomSize / 2)
                    break;
                
                if (targetLobby.Users.Count + lobby.Users.Count <= roomSize / 2)
                {
                    targetLobby.Users.AddRange(lobby.Users);
                    matchedLobbies.Add(lobby);
                    lobbies.Remove(lobby);
                }
                else
                {
                    lobbies.Remove(lobby);
                }
            }
        }
    }

    public async Task<IEnumerable<IGameRoomInstance>> UpdateMatchmakingLobbies(HashSet<Lobby> lobbies, int roomSize)
    {
        HashSet<IGameRoomInstance> rooms = new HashSet<IGameRoomInstance>();

        //TODO: Adjust solo wait matchmaking
        // int expectedRooms = allPlayers.Count / roomSize;
        // if (lobbies.Count > 0)
        // {
        //     double maxWaitTime = lobbies.Max(l => l.WaitTime);
        //     if (maxWaitTime > MAX_SOLO_PLAYER_WAIT_TIME)
        //     {
        //         expectedRooms = 1;
        //         minPlayersAmount = 1;
        //     }
        // }
        
        foreach (GameType gt in System.Enum.GetValues(typeof(GameType)))
        {
            var lobbiesWithGameType = lobbies
                .OrderBy(l => l.WaitTime)
                .Where(l => l.State == MatchmakingState.Searching &&
                            l.GameTypesSelected.Contains(gt))
                .ToList();

            while (lobbiesWithGameType.Count > 0)
            {
                List<Lobby> matchedLobbies = new List<Lobby>();
                Lobby mergedLobby1 = new Lobby();
                Lobby mergedLobby2 = new Lobby();

                PopulateMergedLobby(mergedLobby1, lobbiesWithGameType, matchedLobbies, roomSize);
                PopulateMergedLobby(mergedLobby2, lobbiesWithGameType, matchedLobbies, roomSize);

                if (mergedLobby1.Users.Count == roomSize / 2 && mergedLobby2.Users.Count == roomSize / 2)
                {
                    matchedLobbies.ForEach(l => l.State = MatchmakingState.InGame);

                    var room = _gameRoomService.CreateRoom("room", gt, false);
                    _gameRoomService.StartGameInstance(room);

                    var usersToMatchmake = new List<GameUserService.GameUser>();
                    usersToMatchmake.AddRange(mergedLobby1.Users);
                    usersToMatchmake.AddRange(mergedLobby2.Users);

                    foreach (var user in usersToMatchmake)
                    {
                        room.AddPlayer(user.Username);
                        await _gameHubContext.Clients.Client(user.ConnectionId).SendAsync("JoinGameInstance",
                            JsonConvert.SerializeObject(room.GetDto()));
                        rooms.Add(room);
                    }
                }
            }
        }

        var lobbiesWithoutGame = lobbies.Where(l => l.State == MatchmakingState.Searching).ToList();
        var lobbiesWithGame = lobbies.Where(l => l.State == MatchmakingState.InGame).ToList();
        
        return rooms;
    }

    public void AddUserToLobby(GameUserService.GameUser gameUser, Lobby lobby)
    {
        RemoveUserFromLobby(gameUser);

        gameUser.Lobby = lobby;
        gameUser.Lobby.Users.Add(gameUser);
        
        Lobbies.Add(lobby);
        _logger.LogInformation($"{gameUser.Username} has joined lobby with id {lobby.Id}");
    }

    public void RemoveUserFromLobby(GameUserService.GameUser gameUser)
    {
        var lobby = gameUser.Lobby;
        if (gameUser.Lobby is null) return;
        
        lobby.Users.Remove(gameUser);
        if(gameUser.Lobby.Users.Count == 0) Lobbies.Remove(gameUser.Lobby);

        _logger.LogInformation($"{gameUser.Username} has left lobby with id {lobby.Id}");
    }

    public void StartLobbyMatchmaking(Lobby lobby)
    {
        SetLobbyState(lobby, MatchmakingState.Searching);
        lobby.WaitStartTime = DateTime.UtcNow;
    }

    public void StopLobbyMatchmaking(Lobby lobby)
    {
        SetLobbyState(lobby, MatchmakingState.Idle);
    }

    private async Task SetLobbyState(Lobby lobby, MatchmakingState matchmakingState)
    {
        lobby.State = matchmakingState;
        _logger.LogInformation($"Lobby {lobby.Id} state changed: {lobby.State}");
        await _gameHubContext.Clients.Clients(lobby.Users.Select(u => u.ConnectionId)).SendAsync("SetMatchmakingState",
            matchmakingState == MatchmakingState.Searching ? "true" : "false");
    }
}