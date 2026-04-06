using FluentValidation;
using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Commands;

public record ResolveFailureCommand : IRequest<Result>
{
    public Guid FailureId { get; init; }
    public Guid ResolvedByUserId { get; init; }
    public decimal? RepairCost { get; init; }
    public string? RepairDescription { get; init; }
}

public class ResolveFailureValidator : AbstractValidator<ResolveFailureCommand>
{
    public ResolveFailureValidator()
    {
        RuleFor(x => x.FailureId).NotEmpty();
        RuleFor(x => x.ResolvedByUserId).NotEmpty();
        RuleFor(x => x.RepairDescription).MaximumLength(2000);
        RuleFor(x => x.RepairCost).GreaterThanOrEqualTo(0).When(x => x.RepairCost.HasValue);
    }
}

public class ResolveFailureHandler : IRequestHandler<ResolveFailureCommand, Result>
{
    private readonly IFailureRepository _failureRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveFailureHandler(IFailureRepository failureRepo, IUnitOfWork unitOfWork)
    {
        _failureRepo = failureRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResolveFailureCommand request, CancellationToken cancellationToken)
    {
        var failure = await _failureRepo.GetByIdAsync(request.FailureId, cancellationToken);
        if (failure is null)
            return Result.Failure("Awaria nie została znaleziona.");

        if (failure.Status == FailureStatus.Closed)
            return Result.Failure("Awaria jest już zamknięta.");

        var oldStatus = failure.Status;
        failure.Status = FailureStatus.Resolved;
        failure.RepairCost = request.RepairCost;
        failure.RepairDescription = request.RepairDescription;
        failure.ResolvedAt = DateTime.UtcNow;

        var history = new FailureStatusHistory
        {
            Id = Guid.NewGuid(),
            FailureId = failure.Id,
            OldStatus = oldStatus,
            NewStatus = FailureStatus.Resolved,
            ChangedByUserId = request.ResolvedByUserId,
            Notes = "Awaria rozwiązana",
            CreatedAt = DateTime.UtcNow,
        };

        await _failureRepo.AddStatusHistoryAsync(history, cancellationToken);
        _failureRepo.Update(failure);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
