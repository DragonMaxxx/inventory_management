using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public record DeleteDeviceCommand(Guid Id) : IRequest<Result>;

public class DeleteDeviceHandler : IRequestHandler<DeleteDeviceCommand, Result>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeviceHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
            return Result.Failure("Urządzenie nie zostało znalezione.");

        // Soft-delete: zmiana statusu na Archived
        device.Status = DeviceStatus.Archived;
        _deviceRepo.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
