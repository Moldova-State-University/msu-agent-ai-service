using OllamaSharp;
using OllamaSharp.Models;
using USMAgent.Application;

namespace USMAgent.Infrastructure.AI.Clients
{
    internal class AiClient : IAiClient
    {
        private readonly OllamaApiClient _ollama;
        private readonly Chat _chat;

        public AiClient(OllamaApiClient ollama) 
        { 
            _ollama = ollama;

            // TODO add system prompt
            var chatAi = new Chat(ollama, systemPropt)
            {
                Think = false,
                AllowRecursiveToolCalls = true,
                Options = new RequestOptions
                {
                    // TODO add constants
                    Temperature = 0.2f,
                    TopP = 0.8f,
                    NumCtx = 8192,
                }
            };
        }

        public void SendRequest(string message)
        {
            _chat.SendAsync(message, [new SearchRegulationsTool(), new GetCurrentLessonTool()]))
        }
    }
}