using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using USMAgent.Application;
using USMAgent.Application.Abstractions;
using USMAgent.Infrastructure.AI.Ollama;
using USMAgent.Infrastructure.DependencyInjection;
using USMAgent.Infrastructure.FileSystem;
using USMAgent.Infrastructure.FileSystem.Json;
using USMAgent.Infrastructure.VectorStore.Qdrant;

namespace USMAgent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsmAgent(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.Configure<IndexingOptions>(configuration.GetSection(IndexingOptions.SectionName));
        
        services.AddPersistence(configuration);
        
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;

            return new QdrantClient(options.Host, options.Port);
        });

        services.AddSingleton<IPromptStore, PromptStore>();
        services.AddSingleton<Sha256FileHashService>();
        services.AddSingleton<IRegulationChunkSource, JsonRegulationChunkSource>();

        services.AddSingleton<IProcessedFilesStore>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<IndexingOptions>>().Value;

            return new ProcessedFilesStore(ResolvePath(options.StateFilePath));
        });

        services.AddSingleton<ITextEmbeddingGenerator, OllamaEmbeddingGenerator>();
        services.AddSingleton<IChatAgent, OllamaChatAgent>();

        services.AddSingleton<IRegulationSearchStore, QdrantRegulationSearchStore>();
        services.AddSingleton<IRegulationIndexStore, QdrantRegulationIndexStore>();

        services.AddSingleton<ISearchRegulations, SearchRegulations>();
        services.AddSingleton<RegulationIndexer>();

        return services;
    }

    private static string ResolvePath(string path) => Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
}
