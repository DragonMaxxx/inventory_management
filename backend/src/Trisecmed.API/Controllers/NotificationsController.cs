using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Application.Notifications.Queries;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "EquipmentWorker,EquipmentManager,Administrator")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Historia powiadomień z filtrowaniem</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? type = null,
        [FromQuery] bool? isSent = null)
    {
        var result = await _mediator.Send(new GetNotificationsQuery
        {
            Page = page,
            PageSize = pageSize,
            Type = type,
            IsSent = isSent,
        });
        return Ok(result);
    }
}
