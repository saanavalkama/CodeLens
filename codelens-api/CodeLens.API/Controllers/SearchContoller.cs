using System.Security.Claims;
using CodeLens.Application.DTOs.Search;
using CodeLens.Application.Interfaces.Search;
using CodeLens.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Authorize]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _service;
    public SearchController(
        ISearchService service
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

    [HttpPost("{repoId}/{conversationId}")]


    public async Task<IActionResult>Search(
        Guid repoId, 
        Guid conversationId, 
        [FromBody ]string message,
        CancellationToken ct
        )
    
    {
        var dto = await _service.SearchAsync(GetAndParseUserId(), repoId, conversationId, message,ct);
        return Ok(dto);

    }


    
}
