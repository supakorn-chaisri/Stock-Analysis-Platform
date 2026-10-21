using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class CorporateAction
{
    public Guid Id { get; set; }
    public Symbol Symbol { get; set; } = default!;
    public string ActionType { get; set; } = string.Empty; // 'split', 'dividend', 'rights', 'merger'
    public DateOnly EffectiveDate { get; set; }
    public decimal? Ratio { get; set; } // for splits: new/old (e.g., 2.0 for 2:1)
    public decimal? CashAmount { get; set; } // for dividends
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}