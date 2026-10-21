using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.DashboardLayoutJson).HasColumnType("jsonb").IsRequired().HasDefaultValue("[]");
        builder.Property(u => u.Theme).HasMaxLength(20).IsRequired().HasDefaultValue("system");
        builder.Property(u => u.Timezone).HasMaxLength(50).IsRequired().HasDefaultValue("UTC");
        builder.Property(u => u.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserPreference>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}