using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class ETLDeadLetterConfiguration : IEntityTypeConfiguration<ETLDeadLetter>
{
    public void Configure(EntityTypeBuilder<ETLDeadLetter> builder)
    {
        builder.ToTable("etl_dead_letter");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.RunId).HasMaxLength(26).IsRequired();
        builder.Property(d => d.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(d => d.Timeframe).HasConversion<int>();
        builder.Property(d => d.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(d => d.ErrorMessage).HasColumnType("text").IsRequired();
        builder.Property(d => d.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Property(d => d.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(d => d.ResolvedAt);
        builder.HasOne(d => d.Run)
            .WithMany()
            .HasForeignKey(d => d.RunId)
            .HasPrincipalKey(r => r.RunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => d.RunId);
    }
}