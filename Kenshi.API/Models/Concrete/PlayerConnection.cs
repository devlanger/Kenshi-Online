namespace Kenshi.API.Models.Concrete;

public class PlayerConnection : BaseEntity
{
    public string Ip { get; set; }
    
    public string Username { get; set; }
    
    public DateTimeOffset LoginTime { get; set; }
    
    public DateTimeOffset? LogoutTime { get; set; }
}