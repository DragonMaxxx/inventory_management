using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Application.Failures.Commands;
using Trisecmed.Application.Failures.Queries;
using Trisecmed.Domain.Enums;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class FailuresController : ControllerBase
{
    private readonly IMediator _mediator;

    public FailuresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Zgłoszenie nowej awarii</summary>
    [HttpPost]
    [Authorize(Roles = "Nurse,EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> Create(
        [FromBody] CreateFailureCommand command,
        [FromServices] IValidator<CreateFailureCommand> validator)
    {
        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>Lista awarii z filtrowaniem i paginacją</summary>
    [HttpGet]
    [Authorize(Roles = "Nurse,EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] FailureStatus? status = null,
        [FromQuery] FailurePriority? priority = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? deviceId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new GetFailuresQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Priority = priority,
            DepartmentId = departmentId,
            DeviceId = deviceId,
            Search = search,
            SortBy = sortBy,
            SortDir = sortDir,
        });
        return Ok(result);
    }

    /// <summary>Szczegóły awarii</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Nurse,EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetFailureByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Zmiana statusu awarii</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeFailureStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeFailureStatusCommand(id, request.Status, GetUserId(), request.Notes));
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>Przypisanie serwisanta do awarii</summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> AssignServiceProvider(Guid id, [FromBody] AssignRequest request)
    {
        var result = await _mediator.Send(new AssignServiceProviderCommand(id, request.ServiceProviderId, GetUserId()));
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>Zamknięcie awarii z kosztem naprawy</summary>
    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveFailureCommand command,
        [FromServices] IValidator<ResolveFailureCommand> validator)
    {
        if (id != command.FailureId)
            return BadRequest(new { error = "ID w URL nie zgadza się z ID w body." });

        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>Historia zmian statusu awarii</summary>
    [HttpGet("{id:guid}/history")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var result = await _mediator.Send(new GetFailureHistoryQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}

public record ChangeFailureStatusRequest(FailureStatus Status, string? Notes = null);
public record AssignRequest(Guid ServiceProviderId);
