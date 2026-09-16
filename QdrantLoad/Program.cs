using DotNetEnv;
using LlamaParserV2.Sevices.FileHelper;
using OllamaSharp;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using QdrantLoad.Helpers;
using QdrantLoad.Helpers.FileHelper;
using System.Text;
using System.Text.Json;
Env.Load("../../../../.env");


//var ollamaEndpoint = new Uri("http://localhost:11434");
var ollamaEndpoint = new Uri(Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? throw new KeyNotFoundException());
string ollamaApiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY") ?? "";

var qdrantEndpoint = new Uri("http://localhost:6334");

const string chatModelName = "qwen3.5:4b";
const string embeddingModelName = "qwen3-embedding:4b";
const string collectionName = "regulations";

var clientChat = new OllamaApiClient(ollamaEndpoint, chatModelName);
//chatClient.DefaultRequestHeaders["Authorization"] = $"Bearer {apiKey}";



var clientEmbendding = new OllamaApiClient(ollamaEndpoint, embeddingModelName);

clientEmbendding.DefaultRequestHeaders["Authorization"] = $"Bearer {ollamaApiKey}";


var qdrantClient = new QdrantClient(qdrantEndpoint);

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

var documentRawRoot = Path.GetFullPath(Path.Combine(projectRoot, "Storage", "DocumentPreparedChunks"));


// Service for working with the JSON cache.
var processedFilesStoreQdrant = new ProcessedFilesStore(Path.Combine(projectRoot, "Storage", "State", "processed-files-Qdrant.json"));

var processedFilesDataQdrant = await processedFilesStoreQdrant.LoadAsync();

if (processedFilesDataQdrant.Count == 0) throw new NullReferenceException();

var firstEmbbending = await clientEmbendding.EmbedAsync("Text to store embending number");

Console.WriteLine("Getting of the vector size");

var vectorSize = (uint)firstEmbbending.Embeddings[0].Length;

Console.WriteLine(vectorSize);

if (!await qdrantClient.CollectionExistsAsync(collectionName))
{

    await qdrantClient.CreateCollectionAsync(collectionName, new VectorParams
    {
        Size = vectorSize,
        Distance = Distance.Cosine
    });
}
Console.WriteLine("EMBEDDING STARTING");
var jsonChunksFiles = Directory.EnumerateFiles(documentRawRoot, "*.json", SearchOption.AllDirectories);

foreach (var chunkFilePath in jsonChunksFiles)
{

    // Computes the file hash to check the cache.
    var sha256 = await FileHashHelper.ComputeSha256Async(chunkFilePath);

    // If the file is already in the cache, skip processing.
    if (processedFilesDataQdrant.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine($"Skipping already processed file: {chunkFilePath}");
        continue;
    }

    Console.WriteLine($"Uploading chunks from the file {chunkFilePath}");
    string jsonString = File.ReadAllText(chunkFilePath);

    var records = JsonSerializer.Deserialize<List<RegulationChunk>>(jsonString);

    var points = new List<PointStruct>();

    foreach (var record in records)
    {
        var response = await clientEmbendding.EmbedAsync(record.EmbeddingText);
        float[] vector = response.Embeddings[0];

        var point = new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = vector
        };

        point.Payload["document_id"] = record.DocumentId;
        point.Payload["document_title"] = record.DocumentTitle;
        point.Payload["chunk_index"] = record.ChunkIndex;
        //point.Payload["headings"] = record.Headings;
        point.Payload["heading_path"] = record.HeadingPath;
        point.Payload["chunk_text"] = record.ChunkText;
        point.Payload["language"] = record.Language;
        point.Payload["version"] = record.Version ?? "";
        point.Payload["valid_from"] = record.ValidFrom ?? "";
        point.Payload["source_file"] = record.SourceFile;
        point.Payload["hash"] = record.Hash;

        points.Add(point);


    }

    await qdrantClient.UpsertAsync(collectionName, points);
    Console.WriteLine($"Uploaded points: {points.Count}");

    // Reads file size and last modified date.
    var fileInfo = new FileInfo(chunkFilePath);

    // Adds a new record for the file that was just processed.
    processedFilesDataQdrant.Add(new ProcessedFileRecord
    {
        FileName = Path.GetFileName(chunkFilePath),
        RelativePath = $"QdrantStorage/{collectionName}",
        Sha256 = sha256,
        Size = fileInfo.Length,
        LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
        ProcessedAtUtc = DateTime.UtcNow,
        ProcesedFilename = "qdrantstorage"
    });

    // Saves the updated cache to JSON.
    await processedFilesStoreQdrant.SaveAsync(processedFilesDataQdrant);

}


////////////////////////////
////Rest OllamaServer text//
////////////////////////////
//Env.Load();
//var ollamaEndpoint = new Uri("http://85.120.14.163/ollama/");


////string? apiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY");
//string apiKey = "OLLAMA-API-KEY-FOR-ACCESS-LIMITATION";

//const string chatModelName = "qwen3.5:4b";


//Console.WriteLine("Key: "+apiKey);

//var chatClient = new OllamaApiClient(ollamaEndpoint);

//chatClient.DefaultRequestHeaders["Authorization"] = $"Bearer {apiKey}";

//var models = await chatClient.ListLocalModelsAsync();
//foreach (var model in models)
//{
//    Console.WriteLine($"Name: {model.Name}  Parameter Size:{model.Details.ParameterSize}");
//}



//var response = await clientEmbendding.EmbedAsync("Document: Universitatea de Stat din Moldova\\nPath: Universitatea de Stat din Moldova > REGULAMENT PRIVIND FORMAREA PROFESIONALĂ LA CICLUL I, STUDII SUPERIOARE DE LICENȚĂ ÎN CADRUL UNIVERSITĂȚII DE STAT DIN MOLDOVA > DISPOZIȚII GENERALE\\n\\nText:\\nArt. 1. Prezentul Regulament stabilește cadrul normativ ce reglementează procesul de formare profesională la ciclul I, studii superioare de licență, în cadrul Universității de Stat din Moldova. \"");
//float[] vector = response.Embeddings[0];



Console.WriteLine();