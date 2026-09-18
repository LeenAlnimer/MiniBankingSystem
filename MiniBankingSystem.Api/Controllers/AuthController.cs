using Microsoft.AspNetCore.Mvc;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        string username,
        string email,
        string password)
    {
        var result = await _authService.RegisterAsync(
            username,
            email,
            password);

        return Ok(new
        {
            message = result
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        string username,
        string password)
    {
        var token = await _authService.LoginAsync(
            username,
            password);

        return Ok(new
        {
            token = token
        });
    }
}