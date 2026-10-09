using Projects;

namespace USMAgent.AIService.Hosting.Aspire.Extensions;

internal static class DistributedApplicationBuilderExtensions
{
    private static string AspnetcoreEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? throw new ArgumentException("ASPNETCORE_ENVIRONMENT");

    public static IResourceBuilder<IResourceWithConnectionString> AddUSMAgentDatabase(
        this IDistributedApplicationBuilder builder,
        string password,
        bool persistDatabase)
    {
        var passwordParameter = builder.AddParameter("password", password, secret: true);

        var postgres = builder
            .AddPostgres("usm-agent-db-server", password: passwordParameter, port: 5433)
            .WithDataVolume();

        if (persistDatabase)
        {
            postgres.WithLifetime(ContainerLifetime.Persistent);
            postgres.Resource.Annotations.OfType<EndpointAnnotation>()
                .Single(ep => ep.Name == "tcp").IsProxied = false;
        }

        return postgres.AddDatabase("USMAgentDb", databaseName: "usm_agent");
    }

    public static IResourceBuilder<IResource> AddMigrationService(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> database)
    {
        return builder.AddProject<USMAgent_MigrationService>("usm-agent-migrationservice")
            .WithReference(database)
            .WithEnvironment("DOTNET_ENVIRONMENT", AspnetcoreEnvironment)
            .WaitFor(database);
    }

    public static IResourceBuilder<ProjectResource> AddUSMAgentApi(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> database,
        IResourceBuilder<IResource> migrationService)
    {
        return builder.AddProject<USMAgent_AIService_API>("usm-agent-ai-service")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", AspnetcoreEnvironment)
            .WithReference(database)
            .WaitForCompletion(migrationService);
    }
}