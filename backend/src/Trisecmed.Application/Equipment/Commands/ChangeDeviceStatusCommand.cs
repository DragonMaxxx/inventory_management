using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public record ChangeDeviceStatusCommand(Guid Id, DeviceStatus Status) : IRequest<Result>;

public class ChangeDeviceStatusHandler : IRequestHandler<ChangeDeviceStatusCommand, Result>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeDeviceStatusHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeDeviceStatusCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
            return Result.Failure("Urządzenie nie zostało znalezione.");

        device.Status = request.Status;
        _deviceRepo.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
