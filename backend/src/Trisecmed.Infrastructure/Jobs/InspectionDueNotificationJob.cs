using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trisecmed.Domain.Enums;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Jobs;

public class InspectionDueNotificationJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InspectionDueNotificationJob> _logger;

    public InspectionDueNotificationJob(IServiceScopeFactory scopeFactory, ILogger<InspectionDueNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrisecmedDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var devices = await db.Devices
            .Include(d => d.Department)
            .Where(d => d.NextInspectionDate != null && d.NextInspectionDate <= deadline && d.Status == DeviceStatus.Active)
            .ToListAsync();

        _logger.LogInformation("InspectionDueNotificationJob: {Count} devices due for inspection", devices.Count);

        var workers = await db.Users
            .Where(u => u.IsActive && (u.Role == UserRole.EquipmentWorker || u.Role == UserRole.EquipmentManager))
            .ToListAsync();

        foreach (var device in devices)
        {
            var daysLeft = device.NextInspectionDate!.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
            var status = daysLeft < 0 ? "PRZETERMINOWANY" : $"za {daysLeft} dni";

            var recipients = workers
                .Where(w => w.DepartmentId == null || w.DepartmentId == device.DepartmentId)
                .ToList();

            foreach (var recipient in recipients)
            {
                var subject = $"[Trisecmed] Przegląd urządzenia: {device.Name} ({status})";
                var body = $"""
                    <h2>Przypomnienie o przeglądzie</h2>
                    <p><strong>Urządzenie:</strong> {device.Name} ({device.InventoryNumber})</p>
                    <p><strong>Oddział:</strong> {device.Department?.Name}</p>
                    <p><strong>Data przeglądu:</strong> {device.NextInspectionDate:yyyy-MM-dd}</p>
                    <p><strong>Status:</strong> {status}</p>
                    """;

                await notificationService.SendAndLogAsync(
                    "InspectionDue", recipient.Email, recipient.Id, subject, body);
            }
        }
    }
}
