using StockAnalysis.Domain.Enums;

namespace StockAnalysis.Domain.Entities;

public class FeatureFlag
{
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int RolloutPct { get; set; } = 100;
    public UserTier? TierRestriction { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}