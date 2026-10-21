using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("feature_flags");
        builder.HasKey(f => f.Key);
        builder.Property(f => f.Key).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Enabled).IsRequired().HasDefaultValue(false);
        builder.Property(f => f.RolloutPct).IsRequired().HasDefaultValue(100);
        builder.Property(f => f.TierRestriction).HasConversion<int>();
        builder.Property(f => f.Description).HasColumnType("text");
        builder.Property(f => f.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
    }
}