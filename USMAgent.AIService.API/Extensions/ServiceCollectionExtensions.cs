using USMAgent.Infrastructure;

namespace USMAgent.AIService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsmAgent(configuration);

        return services;
    }
}
