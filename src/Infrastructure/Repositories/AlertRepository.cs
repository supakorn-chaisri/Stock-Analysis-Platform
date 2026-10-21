using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Domain.ValueObjects;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context) => _context = context;

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Alerts.FindAsync([id], ct);

    public async Task<IReadOnlyList<Alert>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.Alerts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Alert>> GetActiveBySymbolAsync(Symbol symbol, CancellationToken ct = default)
        => await _context.Alerts
            .Where(a => a.Symbol == symbol && a.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(Alert alert, CancellationToken ct = default)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Alert alert, CancellationToken ct = default)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _context.Alerts.FindAsync([id], ct);
        if (alert != null)
        {
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task AddTriggerAsync(AlertTrigger trigger, CancellationToken ct = default)
    {
        _context.AlertTriggers.Add(trigger);
        await _context.SaveChangesAsync(ct);
    }
}