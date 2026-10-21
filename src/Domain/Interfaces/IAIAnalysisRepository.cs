using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Interfaces;

public interface IAIAnalysisRepository
{
    Task<AIAnalysis?> GetByRequestHashAsync(string requestHash, CancellationToken ct = default);
    Task<IReadOnlyList<AIAnalysis>> GetBySymbolAsync(Symbol symbol, CancellationToken ct = default);
    Task AddAsync(AIAnalysis analysis, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
}