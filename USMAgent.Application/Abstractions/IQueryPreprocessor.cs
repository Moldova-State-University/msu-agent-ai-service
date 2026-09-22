namespace USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

/// <summary>
/// Turns a raw user question (Russian, English or Romanian) into search texts suitable
/// for the Romanian regulations index.
/// </summary>
public interface IQueryPreprocessor
{
    /// <summary>
    /// Prepares the question for semantic search.
    /// Implementations must not throw on translation failures: they return
    /// <see cref="PreparedQuery.Passthrough"/> so that search still runs on the original text.
    /// </summary>
    /// <param name="question">Raw user question. Must not be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Models.PreparedQuery> PrepareAsync(
        string question,
        CancellationToken cancellationToken = default);
}
