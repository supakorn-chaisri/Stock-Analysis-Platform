using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Infrastructure.Persistence;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(r => r.ParentTokenHash).HasMaxLength(255);
        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.RevokedAt);
        builder.Property(r => r.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.TokenHash).IsUnique();
    }
}