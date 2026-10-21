using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StockAnalysis.Api.Endpoints;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Api.Endpoints;

public static class WatchlistEndpoints
{
    public static IEndpointRouteBuilder MapWatchlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/watchlists")
            .WithTags("Watchlists")
            .RequireAuthorization();

        group.MapGet("/", GetWatchlistsAsync)
            .WithName("GetWatchlists");

        group.MapGet("/{id:guid}", GetWatchlistAsync)
            .WithName("GetWatchlist");

        group.MapPost("/", CreateWatchlistAsync)
            .WithName("CreateWatchlist");

        group.MapPut("/{id:guid}", UpdateWatchlistAsync)
            .WithName("UpdateWatchlist");

        group.MapDelete("/{id:guid}", DeleteWatchlistAsync)
            .WithName("DeleteWatchlist");

        group.MapPost("/{id:guid}/symbols", AddSymbolAsync)
            .WithName("AddSymbolToWatchlist");

        group.MapDelete("/{id:guid}/symbols/{symbol}", RemoveSymbolAsync)
            .WithName("RemoveSymbolFromWatchlist");

        group.MapPut("/{id:guid}/symbols", ReorderSymbolsAsync)
            .WithName("ReorderWatchlistSymbols");

        return app;
    }

    private static async Task<IResult> GetWatchlistsAsync(
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlists = await repo.GetByUserIdAsync(userId, ct);
        var response = watchlists.Select(w => new WatchlistResponse(
            w.Id, w.Name, w.IsDefault, w.Symbols.Select(s => s.Symbol.Value).ToArray(), w.CreatedAt));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetWatchlistAsync(
        Guid id,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault,
            watchlist.Symbols.Select(s => s.Symbol.Value).ToArray(), watchlist.CreatedAt);
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateWatchlistAsync(
        WatchlistCreateRequest request,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var existingDefault = await repo.GetDefaultByUserIdAsync(userId, ct);
        var isDefault = request.IsDefault || existingDefault == null;

        var watchlist = Watchlist.Create(userId, request.Name, isDefault);
        await repo.AddAsync(watchlist, ct);

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault, Array.Empty<string>(), watchlist.CreatedAt);
        return Results.Created($"/api/v1/watchlists/{watchlist.Id}", response);
    }

    private static async Task<IResult> UpdateWatchlistAsync(
        Guid id,
        WatchlistUpdateRequest request,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

                watchlist.UpdateName(request.Name);
        await repo.UpdateAsync(watchlist, ct);

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault,
            watchlist.Symbols.Select(s => s.Symbol.Value).ToArray(), watchlist.CreatedAt);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteWatchlistAsync(
        Guid id,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

        if (watchlist.IsDefault)
            return Results.BadRequest(new { error = "Cannot delete default watchlist" });

        await repo.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddSymbolAsync(
        Guid id,
        WatchlistAddSymbolRequest request,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

        if (!Symbol.TryParse(request.Symbol, out var symbol))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        watchlist.AddSymbol(symbol);
        await repo.UpdateAsync(watchlist, ct);

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault,
            watchlist.Symbols.Select(s => s.Symbol.Value).ToArray(), watchlist.CreatedAt);
        return Results.Ok(response);
    }

    private static async Task<IResult> RemoveSymbolAsync(
        Guid id,
        string symbol,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        watchlist.RemoveSymbol(sym);
        await repo.UpdateAsync(watchlist, ct);

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault,
            watchlist.Symbols.Select(s => s.Symbol.Value).ToArray(), watchlist.CreatedAt);
        return Results.Ok(response);
    }

    private static async Task<IResult> ReorderSymbolsAsync(
        Guid id,
        WatchlistReorderRequest request,
        IWatchlistRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var watchlist = await repo.GetByIdAsync(id, ct);
        if (watchlist == null || watchlist.UserId != userId)
            return Results.NotFound();

        var symbols = new List<Symbol>();
        foreach (var s in request.Symbols)
        {
            if (Symbol.TryParse(s, out var sym))
                symbols.Add(sym);
        }

        watchlist.ReorderSymbols(symbols);
        await repo.UpdateAsync(watchlist, ct);

        var response = new WatchlistResponse(
            watchlist.Id, watchlist.Name, watchlist.IsDefault,
            watchlist.Symbols.Select(s => s.Symbol.Value).ToArray(), watchlist.CreatedAt);
        return Results.Ok(response);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}