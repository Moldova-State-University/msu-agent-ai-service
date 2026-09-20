using Microsoft.AspNetCore.Mvc;
using USMAgent.AIService.API.Responses;
using USMAgent.Application.Abstractions;
using USMAgent.Domain.Enums;

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
        var result = await _search.ExecuteAsync(query, cancellationToken);

        if (!result.Success)
        {
            return Problem(statusCode: ToStatusCode(result.Code), title: result.Message);
        }

        var response = result.Data!
            .Select(item => new RegulationSearchResponse(
                item.DocumentTitle,
                item.HeadingPath,
                item.ChunkText,
                item.SourceFile,
                item.Language))
            .ToList();

        return Ok(response);
    }

    private static int ToStatusCode(ErrorCode? code) => code switch
    {
        ErrorCode.InvalidArguments => StatusCodes.Status400BadRequest,
        ErrorCode.NotFound or ErrorCode.NoSchedule => StatusCodes.Status404NotFound,
        ErrorCode.AmbiguousName => StatusCodes.Status409Conflict,
        ErrorCode.SourceUnavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };
}
