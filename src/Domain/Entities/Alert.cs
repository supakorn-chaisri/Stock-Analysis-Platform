using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Symbol Symbol { get; set; } = default!;
    public AlertCondition Condition { get; set; }
    public Price Threshold { get; set; } = default!;
    public AlertChannel[] Channels { get; set; } = Array.Empty<AlertChannel>();
    public string? WebhookUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastTriggeredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}