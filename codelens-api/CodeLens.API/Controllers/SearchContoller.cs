using CodeLens.Application.DTOs.Search;
using Microsoft.AspNetCore.Mvc;

namespace CodeLens.API.Controllers;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    public SearchController(){}

    [HttpPost("{repoId}/{conversationId}")]
    public Task<IActionResult>Search(
        Guid repoId, 
        Guid conversationId, 
        [FromBody] SearchRequest request,
        CancellationToken ct
        )
    
    {
        //call service
    }

    
}
