using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Domain.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(IdempotencyKey key, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
}