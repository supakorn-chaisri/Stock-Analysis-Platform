using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class UserPreference
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string DashboardLayoutJson { get; set; } = "[]"; // react-grid-layout format
    public string Theme { get; set; } = "system"; // 'light', 'dark', 'system'
    public string Timezone { get; set; } = "UTC";
    public DateTimeOffset UpdatedAt { get; set; }
}