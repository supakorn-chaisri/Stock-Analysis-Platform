using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class CorporateActionConfiguration : IEntityTypeConfiguration<CorporateAction>
{
    public void Configure(EntityTypeBuilder<CorporateAction> builder)
    {
        builder.ToTable("corporate_actions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(c => c.ActionType).HasMaxLength(20).IsRequired();
        builder.Property(c => c.EffectiveDate).IsRequired();
        builder.Property(c => c.Ratio).HasPrecision(10, 6);
        builder.Property(c => c.CashAmount).HasPrecision(18, 6);
        builder.Property(c => c.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.HasIndex(c => new { c.Symbol, c.EffectiveDate }).IsDescending(false);
    }
}