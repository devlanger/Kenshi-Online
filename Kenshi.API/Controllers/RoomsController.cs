using Microsoft.AspNetCore.Mvc;

namespace Kenshi.API.Controllers;

[ApiController]
[Route("api")]
public class RoomsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return Ok();
    }
}