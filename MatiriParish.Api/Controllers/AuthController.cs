using MatiriParish.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MatiriParish.Api.Controllers;

n[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

n    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

n    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Missing username or password");

n        var ok = await _auth.RegisterInitialAdminAsync(req.Username, req.Password);
        return ok ? Ok(new { success = true }) : Conflict(new { success = false });
    }

n    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req.Username, req.Password);
        return Ok(result);
    }
}

public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
