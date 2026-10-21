using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Interfaces;

public interface IWatchlistRepository
{
    Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Watchlist>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Watchlist?> GetDefaultByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Watchlist watchlist, CancellationToken ct = default);
    Task UpdateAsync(Watchlist watchlist, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}