using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Failures.DTOs;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Failures.Queries;

public record GetFailuresQuery : IRequest<PagedResult<FailureDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public FailureStatus? Status { get; init; }
    public FailurePriority? Priority { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? DeviceId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } = "asc";
}

public class GetFailuresHandler : IRequestHandler<GetFailuresQuery, PagedResult<FailureDto>>
{
    private readonly IFailureRepository _failureRepo;

    public GetFailuresHandler(IFailureRepository failureRepo)
    {
        _failureRepo = failureRepo;
    }

    public async Task<PagedResult<FailureDto>> Handle(GetFailuresQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _failureRepo.GetPagedAsync(
            request.Page, request.PageSize,
            request.Status, request.Priority,
            request.DepartmentId, request.DeviceId,
            request.Search, request.SortBy, request.SortDir,
            cancellationToken);

        return new PagedResult<FailureDto>
        {
            Items = items.Select(f => f.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
