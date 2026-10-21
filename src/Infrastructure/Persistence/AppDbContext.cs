using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockAnalysis.Domain.Entities;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;
using StockAnalysis.Infrastructure.Persistence.Configurations;

namespace StockAnalysis.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<WatchlistSymbol> WatchlistSymbols => Set<WatchlistSymbol>();
    public DbSet<DailyBar> DailyBars => Set<DailyBar>();
    public DbSet<IntradayBar> IntradayBars => Set<IntradayBar>();
    public DbSet<Fundamental> Fundamentals => Set<Fundamental>();
    public DbSet<CorporateAction> CorporateActions => Set<CorporateAction>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertTrigger> AlertTriggers => Set<AlertTrigger>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<ETLRun> ETLRuns => Set<ETLRun>();
    public DbSet<ETLDeadLetter> ETLDeadLetters => Set<ETLDeadLetter>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresEnum<Timeframe>();
        builder.HasPostgresEnum<AnalysisType>();
        builder.HasPostgresEnum<AlertCondition>();
        builder.HasPostgresEnum<AlertChannel>();
        builder.HasPostgresEnum<UserTier>();

        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new WatchlistConfiguration());
        builder.ApplyConfiguration(new WatchlistSymbolConfiguration());
        builder.ApplyConfiguration(new DailyBarConfiguration());
        builder.ApplyConfiguration(new IntradayBarConfiguration());
        builder.ApplyConfiguration(new FundamentalConfiguration());
        builder.ApplyConfiguration(new CorporateActionConfiguration());
        builder.ApplyConfiguration(new AIAnalysisConfiguration());
        builder.ApplyConfiguration(new AlertConfiguration());
        builder.ApplyConfiguration(new AlertTriggerConfiguration());
        builder.ApplyConfiguration(new FeatureFlagConfiguration());
        builder.ApplyConfiguration(new ETLRunConfiguration());
        builder.ApplyConfiguration(new ETLDeadLetterConfiguration());
        builder.ApplyConfiguration(new UserPreferenceConfiguration());
        builder.ApplyConfiguration(new IdempotencyKeyConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}