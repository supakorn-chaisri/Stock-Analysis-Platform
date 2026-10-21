using StockAnalysis.Domain.Enums;

namespace StockAnalysis.Api.Endpoints;

public record AIAnalysisRequest(string Symbol, AnalysisType Type, string Horizon);
public record AIAnalysisResponse(Guid Id, string Symbol, AnalysisType Type, string Horizon, string ResponseJson, string ModelVersion, int? TokensUsed, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt);