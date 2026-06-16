using System.Security.Claims;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/user")]
public class UserContoller : ControllerBase
{
    private readonly IUserService _service;

    public UserContoller(
        IUserService service
    )
    {
        _service = service;
    }

      private Guid GetAndParseUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == null || !Guid.TryParse(id, out var parsedId)) 
            throw new UnauthorizedException("No access to this endpoint");
        return parsedId;
    }

    [HttpPut("openai-key")]
    public async Task<IActionResult> CreateOrUpdateOpenAiKey([FromBody] UpdateOpenAiKeyDto dto)
    {
        await _service.CreateOrUpdateKeyAsync(GetAndParseUserId(), dto.Key);
        return NoContent();
    }

    [HttpGet("openai-key")]
    public async Task<IActionResult> GetKeyStatusAsync()
    {
        var hasKey = await _service.KeyStatusAsync(GetAndParseUserId());
        return Ok(new KeyStatusDto(hasKey));
        
    }

}