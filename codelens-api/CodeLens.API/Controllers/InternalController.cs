using System.Reflection.Metadata.Ecma335;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Application.Interfaces.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Route("internal")]
public class InternalController : ControllerBase
{
    private readonly IGitHubAuthService _service;
    private readonly IConfiguration _config;

    private readonly IHashingService _hasher;

    public InternalController(
        IGitHubAuthService service,
        IConfiguration config,
        IHashingService hasher
    )
    {
        _service = service;
        _config = config;
        _hasher = hasher;
    }


    [HttpPost("refresh-token/{userId}")]
    public async Task<IActionResult> RefreshTokens(Guid userId)
    {
        var expectedKey = _config["InternalApi:ApiKey"] ?? throw new Exception("Api key not found");
        var providedKey = Request.Headers["X-Internal-Key"];

        if(providedKey != expectedKey) return Unauthorized();

        var flag = await _service.RefreshUserTokensAsync(userId);

        if(!flag) return StatusCode(500);
       
        return NoContent();
    }

}