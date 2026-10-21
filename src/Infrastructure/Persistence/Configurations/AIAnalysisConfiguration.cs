using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class AIAnalysisConfiguration : IEntityTypeConfiguration<AIAnalysis>
{
    public void Configure(EntityTypeBuilder<AIAnalysis> builder)
    {
        builder.ToTable("ai_analyses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(a => a.AnalysisType).HasConversion<int>().IsRequired();
        builder.Property(a => a.Horizon).HasMaxLength(10).IsRequired();
        builder.Property(a => a.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(a => a.ResponseJson).HasColumnType("jsonb").IsRequired();
        builder.Property(a => a.ModelVersion).HasMaxLength(50).IsRequired();
        builder.Property(a => a.TokensUsed);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.ExpiresAt).IsRequired();
        builder.HasIndex(a => new { a.Symbol, a.AnalysisType, a.Horizon });
        builder.HasIndex(a => a.RequestHash).IsUnique();
    }
}