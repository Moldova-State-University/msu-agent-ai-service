using Microsoft.AspNetCore.Mvc;
using System.Text;
using USMAgent.AIService.API.Contracts;
using USMAgent.Application.Abstractions;

namespace USMAgent.AIService.API.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatAgent _agent;

    public ChatController(IChatAgent agent)
    {
        _agent = agent;
    }

    [HttpGet("models")]
    [ProducesResponseType<IReadOnlyList<ModelResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels(CancellationToken cancellationToken)
    {
        var models = await _agent.ListModelsAsync(cancellationToken);

        var response = models
            .Select(model => new ModelResponse(model.Name, model.ParameterSize, model.Format))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<AskResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ask(
        [FromBody] string question,
        CancellationToken cancellationToken)
    {
        var answer = new StringBuilder();

        await foreach (var token in _agent.AskAsync(question, cancellationToken))
        {
            answer.Append(token);
        }

        return Ok(new AskResponse(answer.ToString()));
    }
}
