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
        int roomSize = 4;
        var allPlayers = Lobbies
            .Where(l => l.State == MatchmakingState.Searching)
            .OrderBy(l => l.WaitStartTime)
            .SelectMany(l => l.Users)
            .ToList();

        HashSet<Lobby> lobbiesToGame = new HashSet<Lobby>();
        for (int i = 0; i < allPlayers.Count; i = i + roomSize)
        {
            var usersToMatchmake = allPlayers.Skip(i).Take(roomSize).ToList();
            if (usersToMatchmake.Count >= 1)
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
    
    public void AddUserToLobby(UserService.User user, Lobby lobby)
    {
        RemoveUserFromLobby(user);

        user.Lobby = lobby;
        user.Lobby.Users.Add(user);
        
        Lobbies.Add(lobby);
        _logger.LogInformation($"{user.Username} has joined lobby with id {lobby.Id}");
    }

    public void RemoveUserFromLobby(UserService.User user)
    {
        var lobby = user.Lobby;
        if (user.Lobby is null) return;
        
        lobby.Users.Remove(user);
        if(user.Lobby.Users.Count == 0) Lobbies.Remove(user.Lobby);

        _logger.LogInformation($"{user.Username} has left lobby with id {lobby.Id}");
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