using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class ETLRunConfiguration : IEntityTypeConfiguration<ETLRun>
{
    public void Configure(EntityTypeBuilder<ETLRun> builder)
    {
        builder.ToTable("etl_runs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RunId).HasMaxLength(26).IsRequired();
        builder.HasIndex(e => e.RunId).IsUnique();
        builder.Property(e => e.JobName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.Property(e => e.SymbolsProcessed).IsRequired();
        builder.Property(e => e.RecordsInserted).IsRequired();
        builder.Property(e => e.RecordsFailed).IsRequired();
        builder.Property(e => e.StartedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.ErrorMessage).HasColumnType("text");
        builder.HasIndex(e => new { e.JobName, e.StartedAt }).IsDescending(false);
    }
}