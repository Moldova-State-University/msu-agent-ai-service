using DotNetEnv;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using System.Net;
using USMAgent.Application;
using USMAgent.Application.Abstractions;
using USMAgent.Infrastructure;
using USMAgent.Infrastructure.AI.Ollama;
using USMAgent.Infrastructure.FileSystem;
using USMAgent.Infrastructure.FileSystem.Json;
using USMAgent.Infrastructure.VectorStore.Qdrant;

Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddUsmAgent(builder.Configuration);

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection(QdrantOptions.SectionName));
builder.Services.Configure<IndexingOptions>(builder.Configuration.GetSection(IndexingOptions.SectionName));

builder.Services.AddSingleton(sp =>
{
    //var options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
    //var parametersSection = builder.Configuration.GetSection("Parameters");
    //var apiKey = parametersSection.GetValue<string>("apiKey");

    //return new QdrantClient(options.Host, options.Port, false, apiKey);

    return new QdrantClient(new Uri("http://localhost:6334/"), apiKey: "super-secret-api-key");
});

builder.Services.AddSingleton<IPromptStore, PromptStore>();
builder.Services.AddSingleton<Sha256FileHashService>();
builder.Services.AddSingleton<IRegulationChunkSource, JsonRegulationChunkSource>();

builder.Services.AddSingleton<IProcessedFilesStore>(sp =>
{
    var options = sp.GetRequiredService<IOptions<IndexingOptions>>().Value;

    return new ProcessedFilesStore(Path.IsPathRooted(options.StateFilePath)
        ? options.StateFilePath
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, options.StateFilePath)));
});

builder.Services.AddSingleton<ITextEmbeddingGenerator, OllamaEmbeddingGenerator>();
builder.Services.AddSingleton<IChatAgent, OllamaChatAgent>();

builder.Services.AddSingleton<IRegulationSearchStore, QdrantRegulationSearchStore>();
builder.Services.AddSingleton<IRegulationIndexStore, QdrantRegulationIndexStore>();

builder.Services.AddSingleton<ISearchRegulations, SearchRegulations>();
builder.Services.AddSingleton<RegulationIndexer>();

using var host = builder.Build();

await host.Services.GetRequiredService<RegulationIndexer>().RunAsync();