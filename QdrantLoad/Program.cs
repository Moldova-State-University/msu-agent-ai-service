using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using USMAgent.Infrastructure;
using USMAgent.Infrastructure.Indexing;

Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddUsmAgent(builder.Configuration);

using var host = builder.Build();

await host.Services.GetRequiredService<RegulationIndexer>().RunAsync();