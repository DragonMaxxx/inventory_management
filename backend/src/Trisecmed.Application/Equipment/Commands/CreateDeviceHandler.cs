using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Result<Guid>>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeviceHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _deviceRepo.GetByInventoryNumberAsync(request.InventoryNumber, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>($"Urządzenie o numerze inwentarzowym '{request.InventoryNumber}' już istnieje.");

        var device = new Device
        {
            Name = request.Name,
            InventoryNumber = request.InventoryNumber,
            SerialNumber = request.SerialNumber,
            Manufacturer = request.Manufacturer,
            Model = request.Model,
            CategoryId = request.CategoryId,
            DepartmentId = request.DepartmentId,
            Status = request.Status,
            PurchaseDate = request.PurchaseDate,
            PurchasePrice = request.PurchasePrice,
            WarrantyExpires = request.WarrantyExpires,
            NextInspectionDate = request.NextInspectionDate,
            Notes = request.Notes,
            // Tymczasowo — docelowo z JWT claimu (Phase 1)
            CreatedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333")
        };

        await _deviceRepo.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(device.Id);
    }
}
