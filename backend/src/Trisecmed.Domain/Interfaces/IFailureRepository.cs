using Trisecmed.Domain.Entities;

namespace Trisecmed.Domain.Interfaces;

public interface IFailureRepository : IRepository<Failure>
{
    Task<IReadOnlyList<Failure>> GetByDeviceAsync(Guid deviceId, CancellationToken ct = default);
}
