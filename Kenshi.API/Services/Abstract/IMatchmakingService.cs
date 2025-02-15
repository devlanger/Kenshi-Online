using Kenshi.API.Models;

namespace Kenshi.API.Services;

public interface IMatchmakingService
{
    public Task<IEnumerable<IGameRoomInstance>> UpdateMatchmakingLobbies(HashSet<Lobby> lobbies, int roomSize);
    
    public void AddUserToLobby(GameUserService.GameUser gameUser, Lobby lobby);
    
    public void RemoveUserFromLobby(GameUserService.GameUser gameUser);

    public void StartLobbyMatchmaking(Lobby lobby);
    
    public void StopLobbyMatchmaking(Lobby lobby);
}