using AiChatExpirement.Services;
using DotNetEnv;
using OllamaSharp;
using OllamaSharp.Models;

Env.Load("../../../../.env");


//var ollamaEndpoint = new Uri("http://localhost:11434");
var ollamaEndpoint = new Uri(Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? throw new KeyNotFoundException());
string ollamaApiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY") ?? "";

const string chatModelName = "qwen3.5:latest";

var ollama = new OllamaApiClient(ollamaEndpoint, chatModelName);

ollama.DefaultRequestHeaders["Authorization"] = $"Bearer {ollamaApiKey}";

if (!await ollama.IsRunningAsync())
{
    throw new InvalidOperationException("Could not connect to USM Ollama server.");
}


IEnumerable<Model> models = await ollama.ListLocalModelsAsync();

foreach (var model in models.OrderBy(m => m.Name))
    Console.WriteLine($"Name: {model.Name}, ParameterSize: {model.Details.ParameterSize}, Format: {model.Details.Format}");


if (Console.ReadLine() == "end") Environment.Exit(0);

var clientChat = ollama;


var projectFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\")); ;

var filePrompt =  File.ReadAllText(Path.Combine(projectFolder,"Storage", "Prompts", "Chat", "ChatBasePrompt.md"));


if (string.IsNullOrEmpty(filePrompt)) throw new Exception("Chat prompt is empty");

string systemPropt = filePrompt;
var chatAi = new Chat(clientChat, systemPropt)
{
    Think = false,
    AllowRecursiveToolCalls = true,
    Options= new RequestOptions
    {
        Temperature = 0.2f,
        TopP = 0.8f,
        NumCtx = 8192,
    }
};





while (true)
{
    var answ = "";
    Console.WriteLine("Ask yout qwestion:");
    string message = Console.ReadLine();
    Console.WriteLine("\n");
    if (message == "end")
    {
        break;
    }

    string fullAnsw = "";

    Console.WriteLine("Agent answer:");
    await foreach (var answerToken in chatAi.SendAsync(message, [new SearchRegulationsTool(), new GetCurrentLessonTool()]))
    {
        Console.Write(answerToken);
        fullAnsw += answerToken;
    }
    Console.WriteLine("\n");

    //chatAi.Messages.Add(new OllamaSharp.Models.Chat.Message { Role = })
}