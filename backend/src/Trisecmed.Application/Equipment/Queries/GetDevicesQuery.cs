using MediatR;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDevicesQuery : IRequest<IReadOnlyList<Device>>;

public class GetDevicesHandler : IRequestHandler<GetDevicesQuery, IReadOnlyList<Device>>
{
    private readonly IDeviceRepository _deviceRepo;

    public GetDevicesHandler(IDeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
    }

    public Task<IReadOnlyList<Device>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
        => _deviceRepo.GetAllAsync(cancellationToken);
}
