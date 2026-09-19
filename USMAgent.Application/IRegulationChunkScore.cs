namespace USMAgent.Application
{
    public interface IRegulationChunkScore
    {
        Task WriteAsync(string outputPath, IReadOnlyCollection<RegulationChunk> chunks);
        Task<List<RegulationChunk>> ReadAsync(string inputPath);
    }
}
