using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class Fundamental
{
    public Guid Id { get; set; }
    public Symbol Symbol { get; set; } = default!;
    public DateOnly PeriodEnd { get; set; }
    public string PeriodType { get; set; } = string.Empty; // 'quarterly' or 'annual'
    public string Metric { get; set; } = string.Empty; // e.g., 'revenue', 'net_income', 'total_assets', 'eps_diluted'
    public decimal Value { get; set; }
    public string Unit { get; set; } = "native"; // 'native', 'millions', 'billions'
    public string Source { get; set; } = string.Empty; // 'twelve_data', 'manual'
    public DateTimeOffset CreatedAt { get; set; }
}