using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Commands;

public record AssignServiceProviderCommand(Guid FailureId, Guid ServiceProviderId, Guid ChangedByUserId) : IRequest<Result>;

public class AssignServiceProviderHandler : IRequestHandler<AssignServiceProviderCommand, Result>
{
    private readonly IFailureRepository _failureRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AssignServiceProviderHandler(IFailureRepository failureRepo, IUnitOfWork unitOfWork)
    {
        _failureRepo = failureRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AssignServiceProviderCommand request, CancellationToken cancellationToken)
    {
        var failure = await _failureRepo.GetByIdAsync(request.FailureId, cancellationToken);
        if (failure is null)
            return Result.Failure("Awaria nie została znaleziona.");

        var oldStatus = failure.Status;
        failure.ServiceProviderId = request.ServiceProviderId;

        if (failure.Status == FailureStatus.Open)
            failure.Status = FailureStatus.InProgress;

        if (failure.Status != oldStatus)
        {
            var history = new FailureStatusHistory
            {
                Id = Guid.NewGuid(),
                FailureId = failure.Id,
                OldStatus = oldStatus,
                NewStatus = failure.Status,
                ChangedByUserId = request.ChangedByUserId,
                Notes = "Przypisano serwisanta",
                CreatedAt = DateTime.UtcNow,
            };
            await _failureRepo.AddStatusHistoryAsync(history, cancellationToken);
        }

        _failureRepo.Update(failure);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
