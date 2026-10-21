using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class WatchlistSymbol
{
    public Guid WatchlistId { get; set; }
    public Watchlist Watchlist { get; set; } = null!;
    public Symbol Symbol { get; set; } = default!;
    public int SortOrder { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}