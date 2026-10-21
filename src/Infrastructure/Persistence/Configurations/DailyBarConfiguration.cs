using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class DailyBarConfiguration : IEntityTypeConfiguration<DailyBar>
{
    public void Configure(EntityTypeBuilder<DailyBar> builder)
    {
        builder.ToTable("daily_bars");
        builder.HasKey(b => new { b.Symbol, b.Timestamp });
        builder.Property(b => b.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(b => b.Timestamp).IsRequired();
        builder.Property(b => b.Open).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.High).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Low).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Close).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.AdjustedClose).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Volume).HasConversion(v => v.Value, v => new Volume(v)).IsRequired();
        builder.HasIndex(b => new { b.Symbol, b.Timestamp }).IsDescending(false);
    }
}