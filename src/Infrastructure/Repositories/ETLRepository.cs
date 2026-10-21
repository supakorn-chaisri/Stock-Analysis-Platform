using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class ETLRepository : IETLRepository
{
    private readonly AppDbContext _context;

    public ETLRepository(AppDbContext context) => _context = context;

    public async Task<ETLRun?> GetByRunIdAsync(string runId, CancellationToken ct = default)
        => await _context.ETLRuns.FirstOrDefaultAsync(r => r.RunId == runId, ct);

    public async Task<IReadOnlyList<ETLRun>> GetByJobNameAsync(string jobName, int limit = 50, CancellationToken ct = default)
        => await _context.ETLRuns
            .Where(r => r.JobName == jobName)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task AddAsync(ETLRun run, CancellationToken ct = default)
    {
        _context.ETLRuns.Add(run);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ETLRun run, CancellationToken ct = default)
    {
        _context.ETLRuns.Update(run);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddDeadLetterAsync(ETLDeadLetter deadLetter, CancellationToken ct = default)
    {
        _context.ETLDeadLetters.Add(deadLetter);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ETLDeadLetter>> GetDeadLettersByRunIdAsync(string runId, CancellationToken ct = default)
        => await _context.ETLDeadLetters
            .Where(d => d.RunId == runId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
}