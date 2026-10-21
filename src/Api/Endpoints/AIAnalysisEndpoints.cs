using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StockAnalysis.Api.Endpoints;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Api.Endpoints;

public static class AIAnalysisEndpoints
{
    public static IEndpointRouteBuilder MapAIAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/ai-analysis")
            .WithTags("AI Analysis")
            .RequireAuthorization();

        group.MapPost("/", AnalyzeAsync)
            .WithName("AnalyzeStock")
            .RequireAuthorization("PremiumOnly");

        group.MapGet("/{symbol}", GetCachedAnalysisAsync)
            .WithName("GetCachedAnalysis")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> AnalyzeAsync(
        AIAnalysisRequest request,
        IAIAnalysisService aiService,
        IAIAnalysisRepository repo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(request.Symbol, out var symbol))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        var userId = GetUserId(user);
        var userTier = user.FindFirst("tier")?.Value;
        if (userTier != UserTier.Premium.ToString())
            return Results.Forbid();

        var cached = await aiService.GetCachedAsync(symbol, request.Type, request.Horizon, ct);
        if (cached != null)
        {
            var cachedResponse = new AIAnalysisResponse(
                cached.Id, cached.Symbol.Value, cached.AnalysisType, cached.Horizon,
                cached.ResponseJson, cached.ModelVersion, cached.TokensUsed,
                cached.CreatedAt, cached.ExpiresAt);
            return Results.Ok(cachedResponse);
        }

        var analysis = await aiService.AnalyzeAsync(symbol, request.Type, request.Horizon, ct);

        var response = new AIAnalysisResponse(
            analysis.Id, analysis.Symbol.Value, analysis.AnalysisType, analysis.Horizon,
            analysis.ResponseJson, analysis.ModelVersion, analysis.TokensUsed,
            analysis.CreatedAt, analysis.ExpiresAt);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCachedAnalysisAsync(
        string symbol,
        string type,
        string horizon,
        IAIAnalysisRepository repo,
        CancellationToken ct)
    {
        if (!Symbol.TryParse(symbol, out var sym))
            return Results.BadRequest(new { error = "Invalid symbol format. Use EXCHANGE:TICKER" });

        if (!Enum.TryParse<AnalysisType>(type, true, out var analysisType))
            return Results.BadRequest(new { error = "Invalid analysis type. Use quick or deep" });

        var analyses = await repo.GetBySymbolAsync(sym, ct);
        var analysis = analyses.FirstOrDefault(a => a.AnalysisType == analysisType && a.Horizon == horizon);
        if (analysis == null)
            return Results.NotFound();

        var response = new AIAnalysisResponse(
            analysis.Id, analysis.Symbol.Value, analysis.AnalysisType, analysis.Horizon,
            analysis.ResponseJson, analysis.ModelVersion, analysis.TokensUsed,
            analysis.CreatedAt, analysis.ExpiresAt);
        return Results.Ok(response);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}