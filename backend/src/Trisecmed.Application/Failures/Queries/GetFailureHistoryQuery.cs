using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Failures.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Queries;

public record GetFailureHistoryQuery(Guid FailureId) : IRequest<Result<IReadOnlyList<FailureStatusHistoryDto>>>;

public class GetFailureHistoryHandler : IRequestHandler<GetFailureHistoryQuery, Result<IReadOnlyList<FailureStatusHistoryDto>>>
{
    private readonly IFailureRepository _failureRepo;

    public GetFailureHistoryHandler(IFailureRepository failureRepo)
    {
        _failureRepo = failureRepo;
    }

    public async Task<Result<IReadOnlyList<FailureStatusHistoryDto>>> Handle(GetFailureHistoryQuery request, CancellationToken cancellationToken)
    {
        var failure = await _failureRepo.GetByIdAsync(request.FailureId, cancellationToken);
        if (failure is null)
            return Result.Failure<IReadOnlyList<FailureStatusHistoryDto>>("Awaria nie została znaleziona.");

        var history = await _failureRepo.GetStatusHistoryAsync(request.FailureId, cancellationToken);
        return Result.Success<IReadOnlyList<FailureStatusHistoryDto>>(
            history.Select(h => h.ToDto()).ToList());
    }
}
