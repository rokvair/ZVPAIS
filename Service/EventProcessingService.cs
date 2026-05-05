using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Services
{
    public class EventProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventProcessingService> _logger;
        private readonly TimeSpan _pollingInterval;
        private readonly TimeSpan _processingDelay;

        public EventProcessingService(
            IServiceScopeFactory scopeFactory,
            ILogger<EventProcessingService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            // DEMO: polling every 5 s, no delay — revert appsettings to DelayMinutes=60, PollingIntervalMinutes=5 (remove PollingIntervalSeconds) after demo
            var seconds = configuration.GetValue<int>("EventProcessing:PollingIntervalSeconds", 0);
            var minutes = configuration.GetValue<int>("EventProcessing:PollingIntervalMinutes", 5);
            _pollingInterval = seconds > 0
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromMinutes(minutes > 0 ? minutes : 1);

            _processingDelay = TimeSpan.FromMinutes(
                configuration.GetValue<int>("EventProcessing:DelayMinutes", 0));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventProcessingService started. Delay: {Delay}, Poll: {Poll}",
                _processingDelay, _pollingInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEventsAsync(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "EventProcessingService encountered an error, retrying next poll.");
                }
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }

        private async Task ProcessPendingEventsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var calcService = scope.ServiceProvider.GetRequiredService<IDamageCalculationService>();

            var cutoff = DateTimeOffset.UtcNow - _processingDelay;

            var pendingEvents = await db.Events
                .Where(e => e.Status == "naujas" && e.CreatedAt <= cutoff)
                .ToListAsync(ct);

            foreach (var ev in pendingEvents)
            {
                try
                {
                    _logger.LogInformation("Auto-processing event #{Id} ({Type})", ev.IdEvent, ev.EventType);

                    if (await db.DamageEvaluations.AnyAsync(r => r.EventId == ev.IdEvent, ct))
                    {
                        ev.Status = "laukia peržiūros";
                        ev.UpdatedAt = DateTimeOffset.UtcNow;
                        await db.SaveChangesAsync(ct);
                        continue;
                    }

                    var breakdown = await calcService.CalculateBreakdownForEvent(ev.IdEvent);

                    db.DamageEvaluations.Add(new DamageEvaluation
                    {
                        EventId = ev.IdEvent,
                        Data = DateTime.UtcNow,
                        ZalosDydis = (double)breakdown.TotalDamage,
                        PiniginisDydis = (double)breakdown.TotalDamage,
                        Notes = $"Automatiškai sugeneruota ataskaita. I_n = {breakdown.IndexingCoefficient}.",
                        CreatedAt = DateTime.UtcNow
                    });

                    ev.Status = "laukia peržiūros";
                    ev.UpdatedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync(ct);

                    _logger.LogInformation("Event #{Id} processed. Damage: {Damage} EUR. Status: laukia peržiūros",
                        ev.IdEvent, breakdown.TotalDamage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-process event #{Id}", ev.IdEvent);
                }
            }
        }
    }
}