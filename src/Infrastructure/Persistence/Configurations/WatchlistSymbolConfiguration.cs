using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Infrastructure.Persistence.Configurations;

public class WatchlistSymbolConfiguration : IEntityTypeConfiguration<WatchlistSymbol>
{
    public void Configure(EntityTypeBuilder<WatchlistSymbol> builder)
    {
        builder.ToTable("watchlist_symbols");
        builder.HasKey(ws => new { ws.WatchlistId, ws.Symbol });
        builder.Property(ws => ws.Symbol)
            .HasConversion(s => s.Value, s => Symbol.Parse(s))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(ws => ws.SortOrder).IsRequired();
        builder.Property(ws => ws.AddedAt).IsRequired();
        builder.HasOne(ws => ws.Watchlist)
            .WithMany(w => w.Symbols)
            .HasForeignKey(ws => ws.WatchlistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}