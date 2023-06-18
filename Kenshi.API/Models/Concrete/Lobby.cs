using Kenshi.API.Models.Enums;
using Kenshi.API.Services;

namespace Kenshi.API.Models;

public class Lobby
{
    public string Id { get; set; }

    public List<UserService.User> Users { get; set; }
    
    public MatchmakingState State { get; set; }

    public Lobby()
    {
        Id = Guid.NewGuid().ToString();
        Users = new List<UserService.User>();
        State = MatchmakingState.Idle;
    }
}