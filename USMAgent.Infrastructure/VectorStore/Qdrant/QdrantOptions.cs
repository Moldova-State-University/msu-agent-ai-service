using System.ComponentModel.DataAnnotations;

namespace USMAgent.Infrastructure.VectorStore.Qdrant;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";

    [Required]
    public string Host { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 6334;

    [Required]
    public string CollectionName { get; init; } = "regulations";
}