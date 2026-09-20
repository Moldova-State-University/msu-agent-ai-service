namespace USMAgent.AIService.API.Responses;

public sealed record RegulationSearchResponse(
    string DocumentTitle,
    string HeadingPath,
    string ChunkText,
    string SourceFile,
    string Language);
