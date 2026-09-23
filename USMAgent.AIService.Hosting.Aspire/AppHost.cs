using Microsoft.Extensions.Configuration;
using USMAgent.AIService.Hosting.Aspire.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var useLocalDatabase = builder.Configuration.GetSection("UseLocalDatabase").Get<bool>();

IResourceBuilder<ProjectResource> api;

if (useLocalDatabase)
{
    var persistDatabase = builder.Configuration.GetSection("Databases:Persist").Get<bool>();
    var password = builder.Configuration.GetSection("Databases:USMAgentDb:Password").Get<string>()!;

    var database = builder.AddUSMAgentDatabase(password, persistDatabase);
    var migrationService = builder.AddMigrationService(database);
    api = builder.AddUSMAgentApi(database, migrationService);
}
else
{
    api = builder.AddProject<Projects.USMAgent_AIService_API>("usm-agent-ai-service");
}

var apiKey = builder.AddParameter("apiKey", secret: true);

var qdrant = builder.AddQdrant("qdrant", apiKey, 6334, 6333)
    .WithLifetime(ContainerLifetime.Persistent);

var ollama = builder.AddOllama("ollama");
var llama3 = ollama.AddModel("llama3");

var qdrantLoader = builder.AddProject<Projects.QdrantLoad>("qdrantLoad")
    .WithReference(qdrant)
    .WaitFor(qdrant);

api.WithReference(qdrant)
    .WithReference(llama3)
    .WaitFor(qdrantLoader)
    .WaitFor(qdrant)
    .WaitFor(llama3);

builder.Build().Run();
