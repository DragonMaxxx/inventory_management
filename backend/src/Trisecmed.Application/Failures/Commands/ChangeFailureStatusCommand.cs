using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Commands;

public record ChangeFailureStatusCommand(Guid Id, FailureStatus NewStatus, Guid ChangedByUserId, string? Notes = null) : IRequest<Result>;

public class ChangeFailureStatusHandler : IRequestHandler<ChangeFailureStatusCommand, Result>
{
    private readonly IFailureRepository _failureRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeFailureStatusHandler(IFailureRepository failureRepo, IUnitOfWork unitOfWork)
    {
        _failureRepo = failureRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeFailureStatusCommand request, CancellationToken cancellationToken)
    {
        var failure = await _failureRepo.GetByIdAsync(request.Id, cancellationToken);
        if (failure is null)
            return Result.Failure("Awaria nie została znaleziona.");

        var oldStatus = failure.Status;
        failure.Status = request.NewStatus;

        var history = new FailureStatusHistory
        {
            Id = Guid.NewGuid(),
            FailureId = failure.Id,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            ChangedByUserId = request.ChangedByUserId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
        };

        await _failureRepo.AddStatusHistoryAsync(history, cancellationToken);
        _failureRepo.Update(failure);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
