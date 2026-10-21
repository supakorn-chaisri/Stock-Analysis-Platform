using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class AlertTrigger
{
    public Guid Id { get; set; }
    public Guid AlertId { get; set; }
    public Alert Alert { get; set; } = null!;
    public DateTimeOffset TriggeredAt { get; set; }
    public Price CurrentValue { get; set; } = default!;
    public AlertChannel Channel { get; set; }
    public bool Delivered { get; set; }
    public string? ErrorMessage { get; set; }
}