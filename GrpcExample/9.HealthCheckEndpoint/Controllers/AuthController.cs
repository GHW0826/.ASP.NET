using GlobalRpcException.Auth;
using GlobalRpcException.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalRpcException.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtProvider _jwt;

    public AuthController(JwtProvider jwt)
    {
        _jwt = jwt;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthUser user)
    {
        Console.WriteLine($">> user.UserId: {user?.UserId}, Role: {user?.Role}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine(">> ModelState invalid");
            return BadRequest(ModelState);
        }

        var token = _jwt.Generate(user);
        return Ok(new { token });
    }
}