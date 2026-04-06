using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trisecmed.Application.Notifications;
using Trisecmed.Domain.Enums;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Jobs;

public class FailureNotificationJob : IFailureNotificationJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FailureNotificationJob> _logger;

    public FailureNotificationJob(IServiceScopeFactory scopeFactory, ILogger<FailureNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid failureId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrisecmedDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var failure = await db.Failures
            .Include(f => f.Device)
            .Include(f => f.Department)
            .Include(f => f.ReportedByUser)
            .FirstOrDefaultAsync(f => f.Id == failureId);

        if (failure is null)
        {
            _logger.LogWarning("FailureNotificationJob: Failure {Id} not found", failureId);
            return;
        }

        var workers = await db.Users
            .Where(u => u.IsActive && (u.Role == UserRole.EquipmentWorker || u.Role == UserRole.EquipmentManager))
            .Where(u => u.DepartmentId == null || u.DepartmentId == failure.DepartmentId)
            .ToListAsync();

        _logger.LogInformation("FailureNotificationJob: Notifying {Count} workers about failure {Id}", workers.Count, failureId);

        foreach (var worker in workers)
        {
            var subject = $"[Trisecmed] Nowa awaria: {failure.Device?.Name} — {failure.Priority}";
            var body = $"""
                <h2>Zgłoszenie awarii</h2>
                <p><strong>Urządzenie:</strong> {failure.Device?.Name} ({failure.Device?.InventoryNumber})</p>
                <p><strong>Oddział:</strong> {failure.Department?.Name}</p>
                <p><strong>Priorytet:</strong> {failure.Priority}</p>
                <p><strong>Zgłosił:</strong> {failure.ReportedByUser?.FirstName} {failure.ReportedByUser?.LastName}</p>
                <p><strong>Opis:</strong> {failure.Description}</p>
                """;

            await notificationService.SendAndLogAsync(
                "FailureReported", worker.Email, worker.Id, subject, body);
        }
    }
}
