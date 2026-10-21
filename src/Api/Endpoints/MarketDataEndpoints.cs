using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAnalysis.Api.Endpoints;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Api.Endpoints;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/market-data")
            .WithTags("Market Data")
            .RequireAuthorization();

        group.MapGet("/quote/{symbol}", GetQuoteAsync)
            .WithName("GetQuote")
            .AllowAnonymous();

        group.MapGet("/bars/daily/{symbol}", GetDailyBarsAsync)
            .WithName("GetDailyBars")
            .AllowAnonymous();

        group.MapGet("/bars/intraday/{symbol}", GetIntradayBarsAsync)
            .WithName("GetIntradayBars")
            .AllowAnonymous();

        group.MapGet("/fundamentals/{symbol}", GetFundamentalsAsync)
            .WithName("GetFundamentals")
            .AllowAnonymous();

        group.MapGet("/corporate-actions/{symbol}", GetCorporateActionsAsync)
            .WithName("GetCorporateActions")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetQuoteAsync(
        string symbol,
        IMarketDataRepository repo,
        IMarketDataProvider provider,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        var quote = await provider.GetQuoteAsync(sym, ct);
        if (quote == null)
            return Results.NotFound(new { error = "Quote not found" });

        var response = new QuoteResponse(
            quote.Symbol.Value,
            quote.Price.Value,
            quote.Change?.Value,
            quote.ChangePercent,
            quote.Volume.Value,
            quote.Timestamp);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetDailyBarsAsync(
        string symbol,
        DateTimeOffset? from,
        DateTimeOffset? to,
        IMarketDataRepository repo,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        var fromDate = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = to ?? DateTimeOffset.UtcNow;

        var bars = await repo.GetDailyBarsAsync(sym, fromDate, toDate, ct);
        var response = bars.Select(b => new BarResponse(
            b.Symbol.Value, b.Timestamp, b.Open.Value, b.High.Value, b.Low.Value, b.Close.Value, b.Volume.Value));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetIntradayBarsAsync(
        string symbol,
        string timeframe,
        DateTimeOffset? from,
        DateTimeOffset? to,
        IMarketDataRepository repo,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        if (!Enum.TryParse<Timeframe>(timeframe, true, out var tf))
            return Results.BadRequest(new { error = "Invalid timeframe. Use 1m, 5m, 15m, 1h, 1d" });

        var fromDate = from ?? DateTimeOffset.UtcNow.AddDays(-7);
        var toDate = to ?? DateTimeOffset.UtcNow;

        var bars = await repo.GetIntradayBarsAsync(sym, tf, fromDate, toDate, ct);
        var response = bars.Select(b => new BarResponse(
            b.Symbol.Value, b.Timestamp, b.Open.Value, b.High.Value, b.Low.Value, b.Close.Value, b.Volume.Value));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetFundamentalsAsync(
        string symbol,
        string periodType,
        IMarketDataRepository repo,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        var fundamentals = await repo.GetFundamentalsAsync(sym, periodType, ct);
        var response = fundamentals.Select(f => new FundamentalResponse(
            f.Symbol.Value, f.PeriodEnd, f.PeriodType, f.Metric, f.Value, f.Unit, f.Source));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCorporateActionsAsync(
        string symbol,
        IMarketDataRepository repo,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        var actions = await repo.GetCorporateActionsAsync(sym, ct);
        var response = actions.Select(a => new CorporateActionResponse(
            a.Symbol.Value, a.ActionType, a.EffectiveDate, a.Ratio, a.CashAmount, a.Currency, a.Description ?? string.Empty));
        return Results.Ok(response);
    }
}