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

    private Guid GetAndParseUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(id == null || !Guid.TryParse(id, out var parsedId)) throw new UnauthorizedException("No access to this endpoint");
        return parsedId;
    }

    [HttpPost("repos/sync")]
    public async Task<IActionResult> SyncReposAsync()
    {
        var dto = await _service.FetchAndReturnReposAsync(GetAndParseUserId());
        return Ok(dto);
    }

    [HttpGet("repos")]
    public async Task<IActionResult> GetRepos()
    {
        var dto = await _service.GetUserReposAsync(GetAndParseUserId());
        return Ok(dto);
    }

    [HttpPost("repos/{repoId}/index")]
    public async Task <IActionResult>IndexRepo(Guid repoId)
    {
        var indexDto = await _service.IndexRepoAsync(repoId, GetAndParseUserId());
        return Ok(indexDto);
    }

    [HttpGet("repos/{repoId}/files")]
    public async Task<IActionResult>GetFiles(Guid repoId)
    {
        var indexDto = await _service.GetFilesByRepoIdAsync(repoId, GetAndParseUserId());
        return Ok(indexDto);
    }

    [HttpGet("{repoId:guid}/status")]
    public async Task<IActionResult> GetStatusAsync(Guid repoId)
    {
        var dto = await _service.GetRepoStatusAsync(GetAndParseUserId(), repoId);
        return Ok(dto);
    }

}