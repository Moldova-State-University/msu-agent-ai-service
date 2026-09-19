using System.Net.Http.Headers;

namespace USMAgent.Infrastructure.AI.Ollama;

public static class OllamaHelper
{

    public static HttpClient CreateOllamaHttpClient(string baseUrl, string apiKey)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        return httpClient;
    }

    public static HttpClient CreateOllamaHttpClient(Uri baseUrl, string apiKey)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = baseUrl
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        return httpClient;
    }

}