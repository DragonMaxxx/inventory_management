using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Infrastructure.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditInterceptor(ICurrentUserService currentUser) => _currentUser = currentUser;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => (AuditAction?)null
            };

            if (action is null) continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId ?? Guid.Empty,
                Action = action.Value,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id,
                IpAddress = _currentUser.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            if (entry.State == EntityState.Modified)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                auditLog.OldValues = JsonSerializer.Serialize(oldValues);
                auditLog.NewValues = JsonSerializer.Serialize(newValues);
            }
            else if (entry.State == EntityState.Added)
            {
                var newValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                auditLog.NewValues = JsonSerializer.Serialize(newValues);
            }

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Count > 0)
            context.Set<AuditLog>().AddRange(auditEntries);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
}
