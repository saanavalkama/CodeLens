using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(
        IAuthService authService,
        IUserService userService
    )
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var rt = Request.Cookies["refresh_token"];

        if(String.IsNullOrEmpty(rt)) return Unauthorized("No refresh token");

        var AuthResponse = await _authService.RefreshTokenAsync(rt);

        Response.Cookies.Append("refresh_token", AuthResponse.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure=false, //for production true
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new{ accessToken = AuthResponse.AccessToken});
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rt = Request.Cookies["refresh_token"];
        if(String.IsNullOrEmpty(rt)) return Unauthorized("No refresh token");
        await _authService.LogoutAsync(rt);
        Response.Cookies.Delete("refresh_token");
        return Ok();
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
       Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var userId);
       //freshest data for github username and user tier
        var dto = await _userService.Me(userId);
        return Ok(dto);
    }
} 