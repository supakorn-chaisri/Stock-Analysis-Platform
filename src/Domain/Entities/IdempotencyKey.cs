namespace StockAnalysis.Domain.Entities;

public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty; // UUID v4
    public string ResponseJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}