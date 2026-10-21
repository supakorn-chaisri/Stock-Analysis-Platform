using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(i => i.Key);
        builder.Property(i => i.Key).HasMaxLength(64).IsRequired();
        builder.Property(i => i.ResponseJson).HasColumnType("jsonb").IsRequired();
        builder.Property(i => i.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(i => i.ExpiresAt).IsRequired();
        builder.HasIndex(i => i.ExpiresAt);
    }
}