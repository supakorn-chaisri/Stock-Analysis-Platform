using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class ETLDeadLetter
{
    public Guid Id { get; set; }
    public string RunId { get; set; } = string.Empty;
    public ETLRun Run { get; set; } = null!;
    public Symbol Symbol { get; set; } = default!;
    public Timeframe? Timeframe { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}