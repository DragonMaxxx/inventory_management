using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trisecmed.Domain.Enums;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Jobs;

public class WarrantyExpirationJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WarrantyExpirationJob> _logger;

    public WarrantyExpirationJob(IServiceScopeFactory scopeFactory, ILogger<WarrantyExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrisecmedDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));

        var devices = await db.Devices
            .Include(d => d.Department)
            .Where(d => d.WarrantyExpires != null && d.WarrantyExpires <= deadline && d.Status == DeviceStatus.Active)
            .ToListAsync();

        _logger.LogInformation("WarrantyExpirationJob: {Count} devices with expiring warranty", devices.Count);

        var managers = await db.Users
            .Where(u => u.IsActive && u.Role == UserRole.EquipmentManager)
            .ToListAsync();

        foreach (var device in devices)
        {
            var daysLeft = device.WarrantyExpires!.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
            var status = daysLeft < 0 ? "WYGASŁA" : $"wygasa za {daysLeft} dni";

            foreach (var manager in managers)
            {
                var subject = $"[Trisecmed] Gwarancja urządzenia: {device.Name} ({status})";
                var body = $"""
                    <h2>Powiadomienie o gwarancji</h2>
                    <p><strong>Urządzenie:</strong> {device.Name} ({device.InventoryNumber})</p>
                    <p><strong>Oddział:</strong> {device.Department?.Name}</p>
                    <p><strong>Gwarancja do:</strong> {device.WarrantyExpires:yyyy-MM-dd}</p>
                    <p><strong>Status:</strong> {status}</p>
                    """;

                await notificationService.SendAndLogAsync(
                    "WarrantyExpiration", manager.Email, manager.Id, subject, body);
            }
        }
    }
}
