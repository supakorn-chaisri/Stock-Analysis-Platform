using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Interfaces;

public interface IMarketDataRepository
{
    Task<IReadOnlyList<DailyBar>> GetDailyBarsAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(Symbol symbol, Timeframe timeframe, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<DailyBar?> GetLatestDailyBarAsync(Symbol symbol, CancellationToken ct = default);
    Task<IntradayBar?> GetLatestIntradayBarAsync(Symbol symbol, Timeframe timeframe, CancellationToken ct = default);
    Task UpsertDailyBarsAsync(IEnumerable<DailyBar> bars, CancellationToken ct = default);
    Task UpsertIntradayBarsAsync(IEnumerable<IntradayBar> bars, CancellationToken ct = default);
    Task<IReadOnlyList<Fundamental>> GetFundamentalsAsync(Symbol symbol, string periodType, CancellationToken ct = default);
    Task UpsertFundamentalsAsync(IEnumerable<Fundamental> fundamentals, CancellationToken ct = default);
    Task<IReadOnlyList<CorporateAction>> GetCorporateActionsAsync(Symbol symbol, CancellationToken ct = default);
    Task UpsertCorporateActionsAsync(IEnumerable<CorporateAction> actions, CancellationToken ct = default);
}