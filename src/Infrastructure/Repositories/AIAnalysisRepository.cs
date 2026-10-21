using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class AIAnalysisRepository : IAIAnalysisRepository
{
    private readonly AppDbContext _context;

    public AIAnalysisRepository(AppDbContext context) => _context = context;

    public async Task<AIAnalysis?> GetByRequestHashAsync(string requestHash, CancellationToken ct = default)
        => await _context.AIAnalyses.FirstOrDefaultAsync(a => a.RequestHash == requestHash, ct);

    public async Task<IReadOnlyList<AIAnalysis>> GetBySymbolAsync(Symbol symbol, CancellationToken ct = default)
        => await _context.AIAnalyses
            .Where(a => a.Symbol == symbol)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(AIAnalysis analysis, CancellationToken ct = default)
    {
        _context.AIAnalyses.Add(analysis);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        var expired = await _context.AIAnalyses
            .Where(a => a.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync(ct);

        if (expired.Count > 0)
        {
            _context.AIAnalyses.RemoveRange(expired);
            await _context.SaveChangesAsync(ct);
        }
    }
}