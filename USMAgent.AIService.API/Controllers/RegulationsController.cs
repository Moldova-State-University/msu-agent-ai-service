using Microsoft.AspNetCore.Mvc;
using USMAgent.AIService.API.Responses;
using USMAgent.Application.Abstractions;

namespace USMAgent.AIService.API.Controllers;

[ApiController]
[Route("api/regulations")]
public sealed class RegulationsController : ControllerBase
{
    private readonly ISearchRegulations _search;

    public RegulationsController(ISearchRegulations search)
    {
        _search = search;
    }

    [HttpPost("search")]
    [ProducesResponseType<IReadOnlyList<RegulationSearchResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromBody] string query,
        CancellationToken cancellationToken)
    {
        var results = await _search.ExecuteAsync(query, cancellationToken);

        var response = results
            .Select(result => new RegulationSearchResponse(
                result.DocumentTitle,
                result.HeadingPath,
                result.ChunkText,
                result.SourceFile,
                result.Language))
            .ToList();

        return Ok(response);
    }
}
