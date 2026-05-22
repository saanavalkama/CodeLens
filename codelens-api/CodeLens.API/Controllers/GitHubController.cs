using System.Security.Claims;
using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Authorize]
[Route("api/github")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _service;
    public GitHubController(
        IGitHubService service
    )
    {
      _service = service;  
    }

    [HttpPost("repos/sync")]
    public async Task<IActionResult> SyncReposAsync()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(id == null) throw new UnauthorizedException("No access to this endpoint");
        Guid.TryParse(id, out var parsedId);

      

        var dto = await _service.GetUserReposAsync(parsedId);

        return Ok(dto);
    }

    [HttpGet("repos")]
    public async Task<IActionResult> GetRepos()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(id == null) throw new UnauthorizedException("No access to this endpoint");
        Guid.TryParse(id, out var parsedId);

        var dto = await _service.GetUserReposAsync(parsedId);
        return Ok(dto);
    }
}