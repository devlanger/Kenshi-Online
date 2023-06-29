namespace Kenshi.API.Models.Concrete;

public class RegisterResponse
{
    public string Message { get; set; }
    
    public UserDto? User { get; set; }
    
    public bool Success { get; set; }
}