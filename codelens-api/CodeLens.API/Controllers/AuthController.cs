using System.ComponentModel.DataAnnotations;
using CodeLens.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService
    )
    {
        _authService = authService;
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
} 