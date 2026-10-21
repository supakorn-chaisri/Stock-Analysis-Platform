using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.MarketData;
using StockAnalysis.Infrastructure.Persistence;
using StockAnalysis.Infrastructure.Repositories;

namespace StockAnalysis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
            }));

        services.AddStackExchangeRedisCache(options =>
        {
            var redisConnection = configuration.GetConnectionString("Redis")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";
            options.Configuration = redisConnection;
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnection = configuration.GetConnectionString("Redis")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(redisConnection);
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWatchlistRepository, WatchlistRepository>();
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<IAIAnalysisRepository, AIAnalysisRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IETLRepository, ETLRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

        // Market Data Providers
        var twelveDataApiKey = configuration["TwelveData:ApiKey"]
            ?? throw new InvalidOperationException("TwelveData API key not configured.");
        services.AddHttpClient<IMarketDataProvider, TwelveDataProvider>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}