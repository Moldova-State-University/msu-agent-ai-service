namespace USMAgent.Infrastructure.VectorStore.Qdrant;

internal static class QdrantPayloadKeys
{
    public const string DocumentId = "document_id";
    public const string DocumentTitle = "document_title";
    public const string ChunkIndex = "chunk_index";
    public const string HeadingPath = "heading_path";
    public const string ChunkText = "chunk_text";
    public const string Language = "language";
    public const string Version = "version";
    public const string ValidFrom = "valid_from";
    public const string SourceFile = "source_file";
    public const string Hash = "hash";
}
