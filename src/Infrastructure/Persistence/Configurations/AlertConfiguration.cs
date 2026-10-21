using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(a => a.Condition).HasConversion<int>().IsRequired();
        builder.Property(a => a.Threshold).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(a => a.Channels)
            .HasConversion(
                v => string.Join(',', v.Select(c => (int)c)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Select(i => (AlertChannel)i).ToArray())
            .IsRequired();
        builder.Property(a => a.WebhookUrl).HasMaxLength(500);
        builder.Property(a => a.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(a => a.LastTriggeredAt);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
        builder.HasIndex(a => new { a.UserId, a.IsActive });
        builder.HasIndex(a => new { a.Symbol, a.IsActive });
    }
}