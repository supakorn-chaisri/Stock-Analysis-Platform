using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly AppDbContext _context;

    public UserPreferenceRepository(AppDbContext context) => _context = context;

    public async Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.UserPreferences.FindAsync([userId], ct);

    public async Task UpsertAsync(UserPreference preference, CancellationToken ct = default)
    {
        var existing = await _context.UserPreferences.FindAsync([preference.UserId], ct);
        if (existing != null)
        {
            existing.DashboardLayoutJson = preference.DashboardLayoutJson;
            existing.Theme = preference.Theme;
            existing.Timezone = preference.Timezone;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _context.UserPreferences.Add(preference);
        }
        await _context.SaveChangesAsync(ct);
    }
}