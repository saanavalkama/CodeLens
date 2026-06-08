using System.Security.Claims;
using CodeLens.Application.Interfaces.Chat;
using CodeLens.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/conversation")]

public class ConversationController : ControllerBase
{
    private readonly IConversationService _service;

    public ConversationController(
        IConversationService service
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

    [HttpPost("{repoId:guid}")]
    public async Task<IActionResult> CreateAsync(Guid repoId)
    {
    
       var dto = await _service.CreateAsync(GetAndParseUserId(), repoId);
       return Ok(dto);

    }

    [HttpGet("{repoId:guid}")]
    public async Task<IActionResult>GetConversationsByRepoId(Guid repoId)
    {
        var dtoList = await _service.GetConversationsByRepoIdAsync(GetAndParseUserId(),repoId);
        return Ok(dtoList);
    }
}