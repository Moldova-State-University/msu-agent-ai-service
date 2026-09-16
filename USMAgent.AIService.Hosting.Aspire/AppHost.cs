var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder.AddQdrant("qdrant")
    .WithLifetime(ContainerLifetime.Persistent);

var ollama = builder.AddOllama("ollama");
var llama3 = ollama.AddModel("llama3");

var aiService = builder.AddProject<Projects.USMAgent_AIService_API>("ai-service-api")
    .WithReference(qdrant)
    .WithReference(llama3)
    .WaitFor(qdrant)
    .WaitFor(llama3);

builder.Build().Run();