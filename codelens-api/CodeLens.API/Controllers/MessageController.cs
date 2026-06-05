using System.Security.Claims;
using CodeLens.Application.DTOs.Chat;
using CodeLens.Application.DTOs.Search;
using CodeLens.Application.Interfaces.Search;
using CodeLens.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _service;
    public MessageController(
        IMessageService service
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
        [FromBody] SendMessageRequestDto request,
        CancellationToken ct
        )

    {
        var dto = await _service.SearchAsync(GetAndParseUserId(), repoId, conversationId, request.Message,ct);
        return Ok(dto);

    }

    [HttpGet("{repoId}/{conversationId}")]
    public async Task <IActionResult>GetMessages(Guid repoId, Guid conversationId)
    {
        var dto = await _service.GetNMessagesAync(GetAndParseUserId(),repoId,conversationId);
        return Ok(dto);
    }


    
}
