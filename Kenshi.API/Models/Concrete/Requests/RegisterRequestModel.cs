namespace Kenshi.API.Models.Concrete.Requests;

public class RegisterRequestModel
{
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public string RepeatPassword { get; set; }
    
    public string Email { get; set; }
}