using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace API.HostedServices;

public sealed class VehicleScheduleAutoRunner : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VehicleScheduleAutoRunner> _logger;

    public VehicleScheduleAutoRunner(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<VehicleScheduleAutoRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("ScheduleAutomation:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("[ScheduleAuto] Disabled by configuration.");
            return;
        }

        var intervalSeconds = _configuration.GetValue<int>("ScheduleAutomation:IntervalSeconds", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ScheduleAuto] Failed to process schedules.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // stop requested
            }
        }
    }

    private async Task ProcessOnceAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CarManagerContext>();

        var actorUserId = _configuration.GetValue<int>("ScheduleAutomation:ActorUserId", 0);
        var now = DateTime.Now;

        var toStart = await db.VehicleSchedules
            .Where(s => s.DeletedAt == null
                && s.Status == "Planned"
                && s.PlannedStartTime <= now
                && s.ActualStartTime == null)
            .ToListAsync(stoppingToken);

        var toEnd = await db.VehicleSchedules
            .Where(s => s.DeletedAt == null
                && s.Status == "InProgress"
                && s.PlannedEndTime <= now
                && s.ActualEndTime == null)
            .ToListAsync(stoppingToken);

        if (toStart.Count == 0 && toEnd.Count == 0)
        {
            return;
        }

        foreach (var schedule in toStart)
        {
            schedule.ActualStartTime = now;
            schedule.Status = "InProgress";
            schedule.UpdatedAt = now;

            if (actorUserId > 0)
            {
                await db.VehicleScheduleAudits.AddAsync(new VehicleScheduleAudit
                {
                    ScheduleId = schedule.Id,
                    ActorUserId = actorUserId,
                    Action = "AutoStart",
                    Note = "Auto start by system job",
                    DataJson = $"{{\"plannedStart\":\"{schedule.PlannedStartTime:O}\"}}",
                    CreatedAt = now
                }, stoppingToken);
            }
        }

        foreach (var schedule in toEnd)
        {
            schedule.ActualEndTime = now;
            schedule.Status = "Completed";
            schedule.UpdatedAt = now;

            if (actorUserId > 0)
            {
                await db.VehicleScheduleAudits.AddAsync(new VehicleScheduleAudit
                {
                    ScheduleId = schedule.Id,
                    ActorUserId = actorUserId,
                    Action = "AutoEnd",
                    Note = "Auto end by system job",
                    DataJson = $"{{\"plannedEnd\":\"{schedule.PlannedEndTime:O}\"}}",
                    CreatedAt = now
                }, stoppingToken);
            }
        }

        await db.SaveChangesAsync(stoppingToken);
    }
}
