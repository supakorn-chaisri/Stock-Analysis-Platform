using System.Text.Json;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.MarketData;

public class TwelveDataProvider : IMarketDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.twelvedata.com";

    public string Name => "TwelveData";

    public TwelveDataProvider(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<MarketDataQuote?> GetQuoteAsync(Symbol symbol, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/quote?symbol={symbol}&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<TwelveDataQuoteResponse>(json, JsonOptions.Default);
        
        if (data?.Close == null) return null;

        return new MarketDataQuote
        {
            Symbol = symbol,
            Price = new Price(data.Close.Value),
            Change = data.Change != null ? new Price(data.Change.Value) : null,
            ChangePercent = data.PercentChange,
            Volume = new Volume(data.Volume ?? 0),
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    public async Task<IReadOnlyList<DailyBar>> GetDailyBarsAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/time_series?symbol={symbol}&interval=1day&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}&apikey={_apiKey}&format=JSON";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return Array.Empty<DailyBar>();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<TwelveDataTimeSeriesResponse>(json, JsonOptions.Default);
        
        if (data?.Values == null) return Array.Empty<DailyBar>();

        return data.Values.Select(v => new DailyBar
        {
            Symbol = symbol,
            Timestamp = DateTimeOffset.Parse(v.Datetime),
            Open = new Price(v.Open),
            High = new Price(v.High),
            Low = new Price(v.Low),
            Close = new Price(v.Close),
            AdjustedClose = new Price(v.Close), // Twelve Data doesn't provide adjusted close separately
            Volume = new Volume(v.Volume)
        }).ToList();
    }

    public async Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(Symbol symbol, Timeframe timeframe, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var interval = timeframe switch
        {
            Timeframe.M1 => "1min",
            Timeframe.M5 => "5min",
            Timeframe.M15 => "15min",
            Timeframe.H1 => "1h",
            _ => "1day"
        };

        var url = $"{_baseUrl}/time_series?symbol={symbol}&interval={interval}&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}&apikey={_apiKey}&format=JSON";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return Array.Empty<IntradayBar>();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<TwelveDataTimeSeriesResponse>(json, JsonOptions.Default);
        
        if (data?.Values == null) return Array.Empty<IntradayBar>();

        return data.Values.Select(v => new IntradayBar
        {
            Symbol = symbol,
            Timeframe = timeframe,
            Timestamp = DateTimeOffset.Parse(v.Datetime),
            Open = new Price(v.Open),
            High = new Price(v.High),
            Low = new Price(v.Low),
            Close = new Price(v.Close),
            Volume = new Volume(v.Volume)
        }).ToList();
    }

    public async Task<IReadOnlyList<Fundamental>> GetFundamentalsAsync(Symbol symbol, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/fundamentals?symbol={symbol}&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return Array.Empty<Fundamental>();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<TwelveDataFundamentalsResponse>(json, JsonOptions.Default);
        
        if (data?.Data == null) return Array.Empty<Fundamental>();

        var result = new List<Fundamental>();
        foreach (var item in data.Data)
        {
            if (DateOnly.TryParse(item.PeriodEnd, out var periodEnd))
            {
                foreach (var metric in item.Metrics)
                {
                    if (decimal.TryParse(metric.Value, out var value))
                    {
                        result.Add(new Fundamental
                        {
                            Id = Guid.NewGuid(),
                            Symbol = symbol,
                            PeriodEnd = periodEnd,
                            PeriodType = item.PeriodType,
                            Metric = metric.Key,
                            Value = value,
                            Unit = "native",
                            Source = "twelve_data",
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }
        }
        return result;
    }

    public async Task<IReadOnlyList<CorporateAction>> GetCorporateActionsAsync(Symbol symbol, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/dividends?symbol={symbol}&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return Array.Empty<CorporateAction>();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<TwelveDataDividendsResponse>(json, JsonOptions.Default);
        
        if (data?.Data == null) return Array.Empty<CorporateAction>();

        return data.Data.Select(d => new CorporateAction
        {
            Id = Guid.NewGuid(),
            Symbol = symbol,
            ActionType = "dividend",
            EffectiveDate = DateOnly.Parse(d.ExDate),
            CashAmount = decimal.TryParse(d.Amount, out var amt) ? amt : null,
            Currency = d.Currency ?? "USD",
            Description = $"Dividend: {d.Amount} {d.Currency}",
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_baseUrl}/quote?symbol=AAPL&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private record TwelveDataQuoteResponse
    {
        public decimal? Close { get; init; }
        public decimal? Change { get; init; }
        public decimal? PercentChange { get; init; }
        public long? Volume { get; init; }
    }

    private record TwelveDataTimeSeriesResponse
    {
        public List<TwelveDataBar>? Values { get; init; }
    }

    private record TwelveDataBar
    {
        public string Datetime { get; init; } = string.Empty;
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public long Volume { get; init; }
    }

    private record TwelveDataFundamentalsResponse
    {
        public List<TwelveDataFundamentalItem>? Data { get; init; }
    }

    private record TwelveDataFundamentalItem
    {
        public string PeriodEnd { get; init; } = string.Empty;
        public string PeriodType { get; init; } = string.Empty;
        public Dictionary<string, string> Metrics { get; init; } = new();
    }

    private record TwelveDataDividendsResponse
    {
        public List<TwelveDataDividend>? Data { get; init; }
    }

    private record TwelveDataDividend
    {
        public string ExDate { get; init; } = string.Empty;
        public string Amount { get; init; } = string.Empty;
        public string? Currency { get; init; }
    }
}