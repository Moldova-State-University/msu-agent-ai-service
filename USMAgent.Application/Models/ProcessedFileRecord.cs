namespace USMAgent.Application.Models;

public sealed class ProcessedFileRecord
{
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastWriteTimeUtc { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
    public string ProcesedFilename { get; set; } = string.Empty;
}
