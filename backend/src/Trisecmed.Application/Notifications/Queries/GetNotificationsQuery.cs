using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Notifications.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Notifications.Queries;

public record GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? Type { get; init; }
    public bool? IsSent { get; init; }
}

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepo;

    public GetNotificationsHandler(INotificationRepository notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _notificationRepo.GetPagedAsync(
            request.Page, request.PageSize, request.Type, request.IsSent, cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(n => n.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
