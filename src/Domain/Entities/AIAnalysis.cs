using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class AIAnalysis
{
    public Guid Id { get; set; }
    public Symbol Symbol { get; set; } = default!;
    public AnalysisType AnalysisType { get; set; }
    public string Horizon { get; set; } = string.Empty; // '1d', '1w', '1m'
    public string RequestHash { get; set; } = string.Empty; // SHA256 of request params
    public string ResponseJson { get; set; } = string.Empty; // structured response per schema
    public string ModelVersion { get; set; } = string.Empty; // e.g., 'gemini-2.5-flash-001'
    public int? TokensUsed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}