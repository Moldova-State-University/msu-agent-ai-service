namespace USMAgent.Application.Models;

public sealed class RegulationChunkDocument
{
    /// <summary>Opaque identifier meaningful only to the source that produced it.</summary>
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Hash { get; init; }
    public required long Size { get; init; }
    public required DateTime LastModifiedUtc { get; init; }
}
