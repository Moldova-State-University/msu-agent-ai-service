namespace USMAgent.Application.Options;

public class IndexingOptions
{
    public const string SectionName = "Indexing";

    public string ChunksPath { get; init; } = "Storage/DocumentPreparedChunks";
    public string StateFilePath { get; init; } = "Storage/State/processed-files-Qdrant.json";
}