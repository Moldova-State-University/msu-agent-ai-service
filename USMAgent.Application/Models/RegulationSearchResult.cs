namespace USMAgent.Application.Models;

public sealed class RegulationSearchResult
{
    public required float Score { get; init; }
    public required string DocumentTitle { get; init; }
    public required string HeadingPath { get; init; }
    public required string ChunkText { get; init; }
    public required string SourceFile { get; init; }
    public required string Language { get; init; }
}