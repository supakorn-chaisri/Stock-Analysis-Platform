using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class AlertTriggerConfiguration : IEntityTypeConfiguration<AlertTrigger>
{
    public void Configure(EntityTypeBuilder<AlertTrigger> builder)
    {
        builder.ToTable("alert_triggers");
        builder.HasKey(at => at.Id);
        builder.Property(at => at.AlertId).IsRequired();
        builder.Property(at => at.TriggeredAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(at => at.CurrentValue).HasConversion(p => p.Value, p => new Price(p)).HasPrecision(18, 6).IsRequired();
        builder.Property(at => at.Channel).HasConversion<int>().IsRequired();
        builder.Property(at => at.Delivered).IsRequired().HasDefaultValue(false);
        builder.Property(at => at.ErrorMessage).HasColumnType("text");
        builder.HasOne(at => at.Alert)
            .WithMany()
            .HasForeignKey(at => at.AlertId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(at => new { at.AlertId, at.TriggeredAt }).IsDescending(false);
    }
}