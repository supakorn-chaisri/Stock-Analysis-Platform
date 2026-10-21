using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Interfaces;

public interface IMarketDataProvider
{
    string Name { get; }
    Task<MarketDataQuote?> GetQuoteAsync(Symbol symbol, CancellationToken ct = default);
    Task<IReadOnlyList<DailyBar>> GetDailyBarsAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(Symbol symbol, Timeframe timeframe, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<Fundamental>> GetFundamentalsAsync(Symbol symbol, CancellationToken ct = default);
    Task<IReadOnlyList<CorporateAction>> GetCorporateActionsAsync(Symbol symbol, CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}

public record MarketDataQuote
{
    public Symbol Symbol { get; init; } = default!;
    public Price Price { get; init; } = default!;
    public Price? Change { get; init; }
    public decimal? ChangePercent { get; init; }
    public Volume Volume { get; init; } = default!;
    public DateTimeOffset Timestamp { get; init; }
}