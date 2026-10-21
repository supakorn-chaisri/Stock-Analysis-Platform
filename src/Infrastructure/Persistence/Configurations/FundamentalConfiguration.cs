using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class FundamentalConfiguration : IEntityTypeConfiguration<Fundamental>
{
    public void Configure(EntityTypeBuilder<Fundamental> builder)
    {
        builder.ToTable("fundamentals");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(f => f.PeriodEnd).IsRequired();
        builder.Property(f => f.PeriodType).HasMaxLength(20).IsRequired();
        builder.Property(f => f.Metric).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Value).HasPrecision(24, 6).IsRequired();
        builder.Property(f => f.Unit).HasMaxLength(20).IsRequired().HasDefaultValue("native");
        builder.Property(f => f.Source).HasMaxLength(50).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.HasIndex(f => new { f.Symbol, f.PeriodEnd, f.PeriodType }).IsUnique();
        builder.HasIndex(f => new { f.Symbol, f.PeriodEnd }).IsDescending(false);
    }
}