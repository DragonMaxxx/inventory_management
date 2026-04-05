using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Equipment.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDeviceInspectionsQuery(Guid DeviceId) : IRequest<Result<IReadOnlyList<InspectionDto>>>;

public class GetDeviceInspectionsHandler : IRequestHandler<GetDeviceInspectionsQuery, Result<IReadOnlyList<InspectionDto>>>
{
    private readonly IDeviceRepository _deviceRepo;

    public GetDeviceInspectionsHandler(IDeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
    }

    public async Task<Result<IReadOnlyList<InspectionDto>>> Handle(GetDeviceInspectionsQuery request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device is null)
            return Result.Failure<IReadOnlyList<InspectionDto>>("Urządzenie nie zostało znalezione.");

        var inspections = await _deviceRepo.GetInspectionsAsync(request.DeviceId, cancellationToken);
        return Result.Success<IReadOnlyList<InspectionDto>>(inspections.Select(i => i.ToDto()).ToList());
    }
}
