using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class IntradayBar
{
    public Symbol Symbol { get; set; } = default!;
    public Timeframe Timeframe { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Price Open { get; set; } = default!;
    public Price High { get; set; } = default!;
    public Price Low { get; set; } = default!;
    public Price Close { get; set; } = default!;
    public Volume Volume { get; set; } = default!;
}