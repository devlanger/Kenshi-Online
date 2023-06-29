namespace Kenshi.API.Models.Concrete.Requests;

public class CheckTokenRequestModel
{
    public string Username { get; set; }
    
    public string Token { get; set; }
}