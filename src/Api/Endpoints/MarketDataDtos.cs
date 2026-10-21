using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Api.Endpoints;

public record QuoteResponse(string Symbol, decimal Price, decimal? Change, decimal? ChangePercent, long Volume, DateTimeOffset Timestamp);
public record BarResponse(string Symbol, DateTimeOffset Timestamp, decimal Open, decimal High, decimal Low, decimal Close, long Volume);
public record FundamentalResponse(string Symbol, DateOnly PeriodEnd, string PeriodType, string Metric, decimal Value, string Unit, string Source);
public record CorporateActionResponse(string Symbol, string ActionType, DateOnly EffectiveDate, decimal? Ratio, decimal? CashAmount, string Currency, string Description);