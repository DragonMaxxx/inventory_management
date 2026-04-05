using FluentValidation;
using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public record UpdateDeviceCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string InventoryNumber { get; init; } = null!;
    public string? SerialNumber { get; init; }
    public string Manufacturer { get; init; } = null!;
    public string Model { get; init; } = null!;
    public Guid CategoryId { get; init; }
    public Guid DepartmentId { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public DateOnly? WarrantyExpires { get; init; }
    public DateOnly? NextInspectionDate { get; init; }
    public string? Notes { get; init; }
}

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Manufacturer).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue);
    }
}

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeviceHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
            return Result.Failure("Urządzenie nie zostało znalezione.");

        // Check unique inventory number (if changed)
        if (device.InventoryNumber != request.InventoryNumber)
        {
            var existing = await _deviceRepo.GetByInventoryNumberAsync(request.InventoryNumber, cancellationToken);
            if (existing is not null)
                return Result.Failure($"Numer inwentarzowy '{request.InventoryNumber}' jest już zajęty.");
        }

        device.Name = request.Name;
        device.InventoryNumber = request.InventoryNumber;
        device.SerialNumber = request.SerialNumber;
        device.Manufacturer = request.Manufacturer;
        device.Model = request.Model;
        device.CategoryId = request.CategoryId;
        device.DepartmentId = request.DepartmentId;
        device.PurchaseDate = request.PurchaseDate;
        device.PurchasePrice = request.PurchasePrice;
        device.WarrantyExpires = request.WarrantyExpires;
        device.NextInspectionDate = request.NextInspectionDate;
        device.Notes = request.Notes;

        _deviceRepo.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
