using FluentValidation;
using Hangfire;
using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Notifications;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Commands;

public record CreateFailureCommand : IRequest<Result<Guid>>
{
    public Guid DeviceId { get; init; }
    public Guid ReportedByUserId { get; init; }
    public Guid DepartmentId { get; init; }
    public string Description { get; init; } = null!;
    public FailurePriority Priority { get; init; } = FailurePriority.Medium;
}

public class CreateFailureValidator : AbstractValidator<CreateFailureCommand>
{
    public CreateFailureValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.ReportedByUserId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class CreateFailureHandler : IRequestHandler<CreateFailureCommand, Result<Guid>>
{
    private readonly IFailureRepository _failureRepo;
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public CreateFailureHandler(
        IFailureRepository failureRepo,
        IDeviceRepository deviceRepo,
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJobClient)
    {
        _failureRepo = failureRepo;
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<Result<Guid>> Handle(CreateFailureCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device is null)
            return Result.Failure<Guid>("Urządzenie nie zostało znalezione.");

        var failure = new Failure
        {
            DeviceId = request.DeviceId,
            ReportedByUserId = request.ReportedByUserId,
            DepartmentId = request.DepartmentId,
            Description = request.Description,
            Priority = request.Priority,
            Status = FailureStatus.Open,
        };

        await _failureRepo.AddAsync(failure, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fire-and-forget notification
        _backgroundJobClient.Enqueue<IFailureNotificationJob>(job => job.ExecuteAsync(failure.Id));

        return Result.Success(failure.Id);
    }
}
