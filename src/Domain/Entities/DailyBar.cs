using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class DailyBar
{
    public Symbol Symbol { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public Price Open { get; set; } = default!;
    public Price High { get; set; } = default!;
    public Price Low { get; set; } = default!;
    public Price Close { get; set; } = default!;
    public Price AdjustedClose { get; set; } = default!;
    public Volume Volume { get; set; } = default!;
}