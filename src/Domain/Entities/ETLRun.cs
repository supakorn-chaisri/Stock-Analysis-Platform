namespace StockAnalysis.Domain.Entities;

public class ETLRun
{
    public Guid Id { get; set; }
    public string RunId { get; set; } = string.Empty; // ULID
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // 'running', 'completed', 'failed', 'partial'
    public int SymbolsProcessed { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsFailed { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}