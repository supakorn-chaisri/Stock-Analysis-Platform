using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Api.Endpoints;

public record WatchlistCreateRequest(string Name, bool IsDefault = false);
public record WatchlistUpdateRequest(string Name);
public record WatchlistAddSymbolRequest(string Symbol);
public record WatchlistReorderRequest(string[] Symbols);
public record WatchlistResponse(Guid Id, string Name, bool IsDefault, string[] Symbols, DateTimeOffset CreatedAt);