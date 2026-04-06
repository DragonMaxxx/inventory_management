using Trisecmed.Domain.Entities;

namespace Trisecmed.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? type, bool? isSent, CancellationToken ct = default);
}
