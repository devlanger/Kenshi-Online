using Kenshi.API.Models;

namespace Kenshi.API.Services;

public interface IMatchmakingService
{
    public Task UpdateMatchmakingLobbies();
    
    public void AddUserToLobby(UserService.User user, Lobby lobby);
    
    public void RemoveUserFromLobby(UserService.User user);

    public void StartLobbyMatchmaking(Lobby lobby);
    
    public void StopLobbyMatchmaking(Lobby lobby);
}