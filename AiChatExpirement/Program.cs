using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using USMAgent.Application.Chat;
using USMAgent.Infrastructure;
using USMAgent.Infrastructure.AI.Ollama.Tools;

const string exitCommand = "end";

Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddUsmAgent(builder.Configuration);

using var host = builder.Build();

RegulationSearchTool.Initialize(host.Services);

var agent = host.Services.GetRequiredService<IChatAgent>();

foreach (var model in await agent.ListModelsAsync())
    Console.WriteLine($"Name: {model.Name}, ParameterSize: {model.ParameterSize}, Format: {model.Format}");

if (Console.ReadLine() == exitCommand) return;

while (true)
{
    Console.WriteLine("Ask your question:");
    var message = Console.ReadLine();
    Console.WriteLine();

    if (string.IsNullOrWhiteSpace(message) || message == exitCommand) break;

    Console.WriteLine("Agent answer:");

    await foreach (var token in agent.AskAsync(message))
        Console.Write(token);

    Console.WriteLine("\n");
}