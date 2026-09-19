// NEW FILE
using System.Security.Cryptography;
using USMAgent.Application.Abstractions;

namespace USMAgent.Infrastructure.FileSystem;

// Contains helper method to compute a file hash.
public class Sha256FileHashService : IFileHashService
{
    // Computes the SHA-256 hash of a file and returns it as a string.
    public async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        // Opens the file for reading.
        await using var stream = File.OpenRead(filePath);
        // Computes the hash from the file contents.
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        // Converts the byte array to a hex string like A1B2C3...
        return Convert.ToHexString(hash);
    }
}
