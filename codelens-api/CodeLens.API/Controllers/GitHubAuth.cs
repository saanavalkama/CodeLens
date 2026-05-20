using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;
using CodeLens.Application.Interfaces.Users;

namespace CodeLens.API.Controllers;

[ApiController]
[Route("api/auth/github")]
public class GitHubAuthContoller : ControllerBase
{
    private readonly IGitHubAuthService _githubService;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    public GitHubAuthContoller(
        IGitHubAuthService githubService,
        IUserService userService,
        IAuthService authService
    )
    {
        _githubService = githubService;
        _userService = userService;
        _authService = authService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var url = _githubService.GetAuthorizationUrl();
        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task <IActionResult>Callback(string code)
    {
        if(string.IsNullOrEmpty(code)) return BadRequest("No code provided");

        //exchanges code for GitHub tokens
        var tokenDto = await _githubService.ExchangeCodeForTokenAsync(code);
        //fetches user, tokens are included in dto
        var gitHubUser = await _githubService.GetUserAsync(tokenDto);
        //saves or updates user, dto is shaped in a way it can be used in claims
        var jwtDto = await _userService.FindOrCreateAsync(gitHubUser);
        //creates applications own tokens
        var tokens = await _authService.HandleGitHubCallbackAsync(jwtDto);

        Response.Cookies.Append("refresh_token", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // set true in production
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new { accessToken = tokens.AccessToken });

    }
}