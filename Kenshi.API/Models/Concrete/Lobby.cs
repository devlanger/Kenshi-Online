using Kenshi.API.Models.Enums;
using Kenshi.API.Services;

namespace Kenshi.API.Models;

public class Lobby
{
    public string Id { get; set; }

    public List<GameUserService.GameUser> Users { get; set; }
    
    public DateTimeOffset WaitStartTime { get; set; }

    public double WaitTime => DateTimeOffset.UtcNow.Subtract(WaitStartTime).TotalSeconds;
    
    public MatchmakingState State { get; set; }

    public Lobby()
    {
        Id = Guid.NewGuid().ToString();
        Users = new List<GameUserService.GameUser>();
        State = MatchmakingState.Idle;
    }
}