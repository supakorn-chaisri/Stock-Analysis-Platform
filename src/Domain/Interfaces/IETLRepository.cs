using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Domain.Interfaces;

public interface IETLRepository
{
    Task<ETLRun?> GetByRunIdAsync(string runId, CancellationToken ct = default);
    Task<IReadOnlyList<ETLRun>> GetByJobNameAsync(string jobName, int limit = 50, CancellationToken ct = default);
    Task AddAsync(ETLRun run, CancellationToken ct = default);
    Task UpdateAsync(ETLRun run, CancellationToken ct = default);
    Task AddDeadLetterAsync(ETLDeadLetter deadLetter, CancellationToken ct = default);
    Task<IReadOnlyList<ETLDeadLetter>> GetDeadLettersByRunIdAsync(string runId, CancellationToken ct = default);
}