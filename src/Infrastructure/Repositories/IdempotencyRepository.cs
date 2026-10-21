using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly AppDbContext _context;

    public IdempotencyRepository(AppDbContext context) => _context = context;

    public async Task<IdempotencyKey?> GetAsync(string key, CancellationToken ct = default)
        => await _context.IdempotencyKeys.FindAsync([key], ct);

    public async Task SetAsync(IdempotencyKey idempotencyKey, CancellationToken ct = default)
    {
        _context.IdempotencyKeys.Add(idempotencyKey);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        var expired = await _context.IdempotencyKeys
            .Where(k => k.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync(ct);

        if (expired.Count > 0)
        {
            _context.IdempotencyKeys.RemoveRange(expired);
            await _context.SaveChangesAsync(ct);
        }
    }
}