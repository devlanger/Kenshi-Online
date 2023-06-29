namespace Kenshi.API.Models.Concrete;

public class CheckTokenResponse
{
    public UserDto? User { get; set; }
    
    public string Message { get; set; }

    public bool Success => User is not null;
}