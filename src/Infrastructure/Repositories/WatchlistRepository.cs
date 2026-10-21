using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly AppDbContext _context;

    public WatchlistRepository(AppDbContext context) => _context = context;

    public async Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Watchlists
            .Include(w => w.Symbols)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<IReadOnlyList<Watchlist>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.Watchlists
            .Include(w => w.Symbols)
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);

    public async Task<Watchlist?> GetDefaultByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.Watchlists
            .Include(w => w.Symbols)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IsDefault, ct);

    public async Task AddAsync(Watchlist watchlist, CancellationToken ct = default)
    {
        _context.Watchlists.Add(watchlist);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Watchlist watchlist, CancellationToken ct = default)
    {
        _context.Watchlists.Update(watchlist);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var watchlist = await _context.Watchlists.FindAsync([id], ct);
        if (watchlist != null)
        {
            _context.Watchlists.Remove(watchlist);
            await _context.SaveChangesAsync(ct);
        }
    }
}