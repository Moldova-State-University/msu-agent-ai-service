using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace USMAgent.Infrastructure.AI.Extentions;

public static class ServiceCollectionExtentions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Configure OLLAMA
        services.AddScoped<OllamaApiClient>();

        return services;
    }
}