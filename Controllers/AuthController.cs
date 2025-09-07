using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _authService.AuthenticateAsync(dto.UserName, dto.Password);
        if (user == null)
            return Unauthorized(new { Message = "Login ou mot de passe incorrect" });

        return Ok(new
        {
            Token = Guid.NewGuid().ToString(), // simplifié
            UserId = user.Id,
            UserName = user.UserName
        });
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Resgister([FromBody] LoginDto dto)
    {
        var user = await _authService.RegisterAsync(dto.UserName, dto.Password);
        if (user)
            return Ok(new { Message = "valider" });
        else
        {
            return Unauthorized(new { Message = "echec" });

        }

    }
}

public class LoginDto
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}