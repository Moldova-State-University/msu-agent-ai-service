namespace USMAgent.AIService.API.Contracts;

public sealed record RegulationSearchResponse(
    string DocumentTitle,
    string HeadingPath,
    string ChunkText,
    string SourceFile,
    string Language);
