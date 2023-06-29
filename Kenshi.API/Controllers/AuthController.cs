using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kenshi.API.Models.Abstract;
using Kenshi.API.Models.Concrete;
using Kenshi.API.Models.Concrete.Requests;
using Kenshi.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Kenshi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private IUserService _userService;

    public AuthController(
        IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Implement logic to fetch data
        var data = "asd";

        // Return data as a JSON response
        return Ok(data);
    }
    
    [HttpPost("check_token")]
    public IActionResult CheckToken([FromBody] CheckTokenRequestModel model)
    {
        CheckTokenResponse response = _userService.CheckToken(model.Username, model.Token);
        
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }

        return Ok(response);
    }
    
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestModel model)
    {
        CheckTokenResponse response = _userService.Authenticate(model);
        
        if (!response.Success)
        {
            return BadRequest("Username or password is incorrect.");
        }

        return Ok(response);
    }
    
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequestModel model)
    {
        var response = _userService.Register(model);
        if (!response.Success)
        {
            return BadRequest("soon");
        }

        return Ok(response);
    }
}