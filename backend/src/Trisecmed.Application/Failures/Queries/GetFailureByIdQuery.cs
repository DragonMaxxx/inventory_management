using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Failures.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Queries;

public record GetFailureByIdQuery(Guid Id) : IRequest<Result<FailureDto>>;

public class GetFailureByIdHandler : IRequestHandler<GetFailureByIdQuery, Result<FailureDto>>
{
    private readonly IFailureRepository _failureRepo;

    public GetFailureByIdHandler(IFailureRepository failureRepo)
    {
        _failureRepo = failureRepo;
    }

    public async Task<Result<FailureDto>> Handle(GetFailureByIdQuery request, CancellationToken cancellationToken)
    {
        var failure = await _failureRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (failure is null)
            return Result.Failure<FailureDto>("Awaria nie została znaleziona.");

        return Result.Success(failure.ToDto());
    }
}
