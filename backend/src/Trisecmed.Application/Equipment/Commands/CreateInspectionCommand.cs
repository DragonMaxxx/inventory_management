using FluentValidation;
using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public record CreateInspectionCommand : IRequest<Result<Guid>>
{
    public Guid DeviceId { get; init; }
    public DateOnly InspectionDate { get; init; }
    public DateOnly? NextInspectionDate { get; init; }
    public string? Result { get; init; }
    public string? Notes { get; init; }
    public string? PerformedBy { get; init; }
}

public class CreateInspectionValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.InspectionDate).NotEmpty();
        RuleFor(x => x.PerformedBy).MaximumLength(255);
    }
}

public class CreateInspectionHandler : IRequestHandler<CreateInspectionCommand, Result<Guid>>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInspectionHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device is null)
            return Result.Failure<Guid>("Urządzenie nie zostało znalezione.");

        var inspection = new Inspection
        {
            DeviceId = request.DeviceId,
            InspectionDate = request.InspectionDate,
            NextInspectionDate = request.NextInspectionDate,
            Result = request.Result,
            Notes = request.Notes,
            PerformedBy = request.PerformedBy,
            // TODO: z JWT claimu po integracji z currentUser
            CreatedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        };

        await _deviceRepo.AddInspectionAsync(inspection, cancellationToken);

        // Update next inspection date on device if provided
        if (request.NextInspectionDate.HasValue)
        {
            device.NextInspectionDate = request.NextInspectionDate;
            _deviceRepo.Update(device);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(inspection.Id);
    }
}
