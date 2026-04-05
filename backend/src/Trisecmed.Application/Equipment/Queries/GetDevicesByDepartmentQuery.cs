using MediatR;
using Trisecmed.Application.Equipment.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDevicesByDepartmentQuery(Guid DepartmentId) : IRequest<IReadOnlyList<DeviceDto>>;

public class GetDevicesByDepartmentHandler : IRequestHandler<GetDevicesByDepartmentQuery, IReadOnlyList<DeviceDto>>
{
    private readonly IDeviceRepository _deviceRepo;

    public GetDevicesByDepartmentHandler(IDeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
    }

    public async Task<IReadOnlyList<DeviceDto>> Handle(GetDevicesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var devices = await _deviceRepo.GetByDepartmentAsync(request.DepartmentId, cancellationToken);
        return devices.Select(d => d.ToDto()).ToList();
    }
}
