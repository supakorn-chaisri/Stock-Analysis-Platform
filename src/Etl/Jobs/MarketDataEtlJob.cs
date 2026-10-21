using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockAnalysis.Domain.Interfaces;
using StockAnalysis.Infrastructure.MarketData;

namespace StockAnalysis.Etl.Jobs;

public class MarketDataEtlJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketDataEtlJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public MarketDataEtlJob(IServiceProvider serviceProvider, ILogger<MarketDataEtlJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Market Data ETL Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunEtlAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Market Data ETL");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunEtlAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IMarketDataProvider>();
        var repo = scope.ServiceProvider.GetRequiredService<IMarketDataRepository>();
        var etlRepo = scope.ServiceProvider.GetRequiredService<IETLRepository>();

        var runId = Ulid.NewUlid().ToString();
        var run = new StockAnalysis.Domain.Entities.ETLRun
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            JobName = "MarketDataEtl",
            Status = "running",
            StartedAt = DateTimeOffset.UtcNow
        };

        await etlRepo.AddAsync(run, ct);

        try
        {
            var symbols = new[] { "SET:PTT", "SET:KBANK", "SET:SCB", "SET:CPALL", "SET:ADVANC" };
            int totalInserted = 0;
            int totalFailed = 0;

            foreach (var symbolStr in symbols)
            {
                if (!StockAnalysis.Domain.ValueObjects.Symbol.TryParse(symbolStr, out var symbol))
                    continue;

                try
                {
                    var bars = await provider.GetDailyBarsAsync(symbol, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow, ct);
                    if (bars.Count > 0)
                    {
                        await repo.UpsertDailyBarsAsync(bars, ct);
                        totalInserted += bars.Count;
                    }
                    run.SymbolsProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process symbol {Symbol}", symbolStr);
                    totalFailed++;
                    
                    var deadLetter = new StockAnalysis.Domain.Entities.ETLDeadLetter
                    {
                        Id = Guid.NewGuid(),
                        RunId = runId,
                        Symbol = symbol,
                        PayloadJson = "{}",
                        ErrorMessage = ex.Message,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await etlRepo.AddDeadLetterAsync(deadLetter, ct);
                }
            }

            run.Status = totalFailed > 0 ? "partial" : "completed";
            run.RecordsInserted = totalInserted;
            run.RecordsFailed = totalFailed;
            run.CompletedAt = DateTimeOffset.UtcNow;
            await etlRepo.UpdateAsync(run, ct);

            _logger.LogInformation("Market Data ETL completed. Inserted: {Inserted}, Failed: {Failed}", totalInserted, totalFailed);
        }
        catch (Exception ex)
        {
            run.Status = "failed";
            run.ErrorMessage = ex.Message;
            run.CompletedAt = DateTimeOffset.UtcNow;
            await etlRepo.UpdateAsync(run, ct);
            throw;
        }
    }
}