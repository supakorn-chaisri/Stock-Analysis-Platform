using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Domain.Interfaces;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(UserPreference preference, CancellationToken ct = default);
}