using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDeviceFailuresQuery(Guid DeviceId) : IRequest<Result<IReadOnlyList<Failure>>>;

public class GetDeviceFailuresHandler : IRequestHandler<GetDeviceFailuresQuery, Result<IReadOnlyList<Failure>>>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IFailureRepository _failureRepo;

    public GetDeviceFailuresHandler(IDeviceRepository deviceRepo, IFailureRepository failureRepo)
    {
        _deviceRepo = deviceRepo;
        _failureRepo = failureRepo;
    }

    public async Task<Result<IReadOnlyList<Failure>>> Handle(GetDeviceFailuresQuery request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device is null)
            return Result.Failure<IReadOnlyList<Failure>>("Urządzenie nie zostało znalezione.");

        var failures = await _failureRepo.GetByDeviceAsync(request.DeviceId, cancellationToken);
        return Result.Success(failures);
    }
}
