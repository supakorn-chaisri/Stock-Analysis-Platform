using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Interfaces;

public interface IAIAnalysisService
{
    Task<AIAnalysis> AnalyzeAsync(Symbol symbol, AnalysisType type, string horizon, CancellationToken ct = default);
    Task<AIAnalysis?> GetCachedAsync(Symbol symbol, AnalysisType type, string horizon, CancellationToken ct = default);
}