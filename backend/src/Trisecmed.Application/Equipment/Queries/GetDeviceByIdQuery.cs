using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Equipment.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDeviceByIdQuery(Guid Id) : IRequest<Result<DeviceDto>>;

public class GetDeviceByIdHandler : IRequestHandler<GetDeviceByIdQuery, Result<DeviceDto>>
{
    private readonly IDeviceRepository _deviceRepo;

    public GetDeviceByIdHandler(IDeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
    }

    public async Task<Result<DeviceDto>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (device is null)
            return Result.Failure<DeviceDto>("Urządzenie nie zostało znalezione.");

        return Result.Success(device.ToDto());
    }
}
