using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Domain.Interfaces;

public interface IFailureRepository : IRepository<Failure>
{
    Task<IReadOnlyList<Failure>> GetByDeviceAsync(Guid deviceId, CancellationToken ct = default);
    Task<Failure?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Failure> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        FailureStatus? status, FailurePriority? priority,
        Guid? departmentId, Guid? deviceId,
        string? search, string? sortBy, string? sortDir,
        CancellationToken ct = default);
    Task<IReadOnlyList<FailureStatusHistory>> GetStatusHistoryAsync(Guid failureId, CancellationToken ct = default);
    Task<FailureStatusHistory> AddStatusHistoryAsync(FailureStatusHistory history, CancellationToken ct = default);
}
