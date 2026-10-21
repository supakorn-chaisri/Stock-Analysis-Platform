using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class MarketDataRepository : IMarketDataRepository
{
    private readonly AppDbContext _context;

    public MarketDataRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<DailyBar>> GetDailyBarsAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await _context.DailyBars
            .Where(b => b.Symbol == symbol && b.Timestamp >= from && b.Timestamp <= to)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(Symbol symbol, Timeframe timeframe, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await _context.IntradayBars
            .Where(b => b.Symbol == symbol && b.Timeframe == timeframe && b.Timestamp >= from && b.Timestamp <= to)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(ct);

    public async Task<DailyBar?> GetLatestDailyBarAsync(Symbol symbol, CancellationToken ct = default)
        => await _context.DailyBars
            .Where(b => b.Symbol == symbol)
            .OrderByDescending(b => b.Timestamp)
            .FirstOrDefaultAsync(ct);

    public async Task<IntradayBar?> GetLatestIntradayBarAsync(Symbol symbol, Timeframe timeframe, CancellationToken ct = default)
        => await _context.IntradayBars
            .Where(b => b.Symbol == symbol && b.Timeframe == timeframe)
            .OrderByDescending(b => b.Timestamp)
            .FirstOrDefaultAsync(ct);

    public async Task UpsertDailyBarsAsync(IEnumerable<DailyBar> bars, CancellationToken ct = default)
    {
        var barsList = bars.ToList();
        if (barsList.Count == 0) return;

        foreach (var bar in barsList)
        {
            var existing = await _context.DailyBars
                .FindAsync([bar.Symbol, bar.Timestamp], ct);

            if (existing != null)
            {
                existing.Open = bar.Open;
                existing.High = bar.High;
                existing.Low = bar.Low;
                existing.Close = bar.Close;
                existing.AdjustedClose = bar.AdjustedClose;
                existing.Volume = bar.Volume;
            }
            else
            {
                _context.DailyBars.Add(bar);
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpsertIntradayBarsAsync(IEnumerable<IntradayBar> bars, CancellationToken ct = default)
    {
        var barsList = bars.ToList();
        if (barsList.Count == 0) return;

        foreach (var bar in barsList)
        {
            var existing = await _context.IntradayBars
                .FindAsync([bar.Symbol, bar.Timeframe, bar.Timestamp], ct);

            if (existing != null)
            {
                existing.Open = bar.Open;
                existing.High = bar.High;
                existing.Low = bar.Low;
                existing.Close = bar.Close;
                existing.Volume = bar.Volume;
            }
            else
            {
                _context.IntradayBars.Add(bar);
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Fundamental>> GetFundamentalsAsync(Symbol symbol, string periodType, CancellationToken ct = default)
        => await _context.Fundamentals
            .Where(f => f.Symbol == symbol && f.PeriodType == periodType)
            .OrderByDescending(f => f.PeriodEnd)
            .ToListAsync(ct);

    public async Task UpsertFundamentalsAsync(IEnumerable<Fundamental> fundamentals, CancellationToken ct = default)
    {
        var list = fundamentals.ToList();
        if (list.Count == 0) return;

        foreach (var f in list)
        {
            var existing = await _context.Fundamentals
                .FirstOrDefaultAsync(x => x.Symbol == f.Symbol && x.PeriodEnd == f.PeriodEnd && x.PeriodType == f.PeriodType && x.Metric == f.Metric, ct);

            if (existing != null)
            {
                existing.Value = f.Value;
                existing.Unit = f.Unit;
                existing.Source = f.Source;
            }
            else
            {
                _context.Fundamentals.Add(f);
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CorporateAction>> GetCorporateActionsAsync(Symbol symbol, CancellationToken ct = default)
        => await _context.CorporateActions
            .Where(c => c.Symbol == symbol)
            .OrderByDescending(c => c.EffectiveDate)
            .ToListAsync(ct);

    public async Task UpsertCorporateActionsAsync(IEnumerable<CorporateAction> actions, CancellationToken ct = default)
    {
        var list = actions.ToList();
        if (list.Count == 0) return;

        foreach (var a in list)
        {
            var existing = await _context.CorporateActions
                .FirstOrDefaultAsync(x => x.Symbol == a.Symbol && x.ActionType == a.ActionType && x.EffectiveDate == a.EffectiveDate, ct);

            if (existing != null)
            {
                existing.Ratio = a.Ratio;
                existing.CashAmount = a.CashAmount;
                existing.Currency = a.Currency;
                existing.Description = a.Description;
            }
            else
            {
                _context.CorporateActions.Add(a);
            }
        }
        await _context.SaveChangesAsync(ct);
    }
}