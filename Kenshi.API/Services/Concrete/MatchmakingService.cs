using Kenshi.API.Hub;
using Kenshi.API.Models;
using Kenshi.API.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Kenshi.API.Services.Concrete;

public class MatchmakingService : IMatchmakingService
{
    public static HashSet<Lobby> Lobbies { get; set; }
    
    private readonly ILogger<MatchmakingService> _logger;
    private readonly IGameRoomService _gameRoomService;
    private readonly IHubContext<GameHub> _gameHubContext;

    private const float MAX_SOLO_PLAYER_WAIT_TIME = 20;
    private const int MIN_PLAYERS_AMOUNT_TO_START_GAMEROOM = 2;
    private const int PLAYERS_ROOM_SIZE_TO_TAKE_FOR_GAME = 4;

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
    
    public async Task UpdateMatchmakingLobbies()
    {
        int minPlayersAmount = MIN_PLAYERS_AMOUNT_TO_START_GAMEROOM;
        int roomSize = PLAYERS_ROOM_SIZE_TO_TAKE_FOR_GAME;
        var allPlayers = Lobbies
            .Where(l => l.State == MatchmakingState.Searching)
            .OrderBy(l => l.WaitStartTime)
            .SelectMany(l => l.Users)
            .ToList();

        HashSet<Lobby> lobbiesToGame = new HashSet<Lobby>();
        for (int i = 0; i < allPlayers.Count; i = i + roomSize)
        {
            var usersToMatchmake = allPlayers.Skip(i).Take(roomSize).ToList();
            if(usersToMatchmake.Count > 0)
            {
                //If user is waiting more than 60 seconds and nobody is in the game
                //Connect him to play with the bots
                if (usersToMatchmake[0].Lobby.WaitTime > MAX_SOLO_PLAYER_WAIT_TIME)
                {
                    minPlayersAmount = 1;
                }
            }
            
            if (usersToMatchmake.Count >= minPlayersAmount)
            {
                var room = _gameRoomService.CreateRoom("room", false);
                _gameRoomService.StartGameInstance(room);

                foreach (var user in usersToMatchmake)
                {
                    room.AddPlayer(user.Username);
                    await _gameHubContext.Clients.Client(user.ConnectionId).SendAsync("JoinGameInstance",
                        JsonConvert.SerializeObject(room.GetDto()));
                    lobbiesToGame.Add(user.Lobby);
                }
            }
        }

        foreach (var lobby in lobbiesToGame)
        {
            SetLobbyState(lobby, MatchmakingState.InGame);
        }
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
        await _gameHubContext.Clients.Clients(lobby.Users.Select(u => u.ConnectionId)).SendAsync("SetMatchmakingState", matchmakingState == MatchmakingState.Searching ? "true" : "false");
    }
}