using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Equipment.DTOs;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Queries;

public record GetDevicesQuery : IRequest<PagedResult<DeviceDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } = "asc";
    public DeviceStatus? Status { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Search { get; init; }
}

public class GetDevicesHandler : IRequestHandler<GetDevicesQuery, PagedResult<DeviceDto>>
{
    private readonly IDeviceRepository _deviceRepo;

    public GetDevicesHandler(IDeviceRepository deviceRepo)
    {
        _deviceRepo = deviceRepo;
    }

    public async Task<PagedResult<DeviceDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var (devices, totalCount) = await _deviceRepo.GetPagedAsync(
            page: request.Page,
            pageSize: Math.Clamp(request.PageSize, 1, 100),
            status: request.Status,
            departmentId: request.DepartmentId,
            categoryId: request.CategoryId,
            search: request.Search,
            sortBy: request.SortBy,
            sortDir: request.SortDir,
            ct: cancellationToken);

        return new PagedResult<DeviceDto>
        {
            Items = devices.Select(d => d.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
