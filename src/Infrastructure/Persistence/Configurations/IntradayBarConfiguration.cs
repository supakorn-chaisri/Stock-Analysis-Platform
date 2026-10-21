using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class IntradayBarConfiguration : IEntityTypeConfiguration<IntradayBar>
{
    public void Configure(EntityTypeBuilder<IntradayBar> builder)
    {
        builder.ToTable("intraday_bars");
        builder.HasKey(b => new { b.Symbol, b.Timeframe, b.Timestamp });
        builder.Property(b => b.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(b => b.Timeframe).HasConversion<int>().IsRequired();
        builder.Property(b => b.Timestamp).IsRequired();
        builder.Property(b => b.Open).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.High).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Low).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Close).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.Volume).HasConversion(v => v.Value, v => new Volume(v)).IsRequired();
        builder.HasIndex(b => new { b.Symbol, b.Timeframe, b.Timestamp }).IsDescending(false);
    }
}