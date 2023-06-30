namespace Kenshi.API.Models.Concrete;

public class User : BaseEntity
{
    public string Email { get; set; }
    
    public string Username { get; set; }
    
    public string PasswordHash { get; set; }
    
    public string? Token { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTimeOffset? TokenExpirationDate { get; set; }
    
    public bool IsActivated { get; set; }
}