using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Application.Equipment.Commands;
using Trisecmed.Application.Equipment.Queries;
using Trisecmed.Domain.Enums;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista urządzeń z filtrowaniem, sortowaniem i paginacją</summary>
    [HttpGet]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] DeviceStatus? status = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetDevicesQuery
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDir = sortDir,
            Status = status,
            DepartmentId = departmentId,
            CategoryId = categoryId,
            Search = search,
        });
        return Ok(result);
    }

    /// <summary>Szczegóły urządzenia</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Nurse,EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDeviceByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Urządzenia przypisane do oddziału</summary>
    [HttpGet("department/{departmentId:guid}")]
    [Authorize(Roles = "Nurse,EquipmentWorker,EquipmentManager,Administrator")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId)
    {
        var result = await _mediator.Send(new GetDevicesByDepartmentQuery(departmentId));
        return Ok(result);
    }

    /// <summary>Dodanie nowego urządzenia</summary>
    [HttpPost]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeviceCommand command,
        [FromServices] IValidator<CreateDeviceCommand> validator)
    {
        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Conflict(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>Aktualizacja danych urządzenia</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDeviceCommand command,
        [FromServices] IValidator<UpdateDeviceCommand> validator)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID w URL nie zgadza się z ID w body." });

        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>Zmiana statusu urządzenia (np. disposed wymaga roli manager)</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "EquipmentManager")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeDeviceStatusCommand(id, request.Status));
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>Soft-delete (archiwizacja) urządzenia</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "EquipmentManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDeviceCommand(id));
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>Historia przeglądów urządzenia</summary>
    [HttpGet("{id:guid}/inspections")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> GetInspections(Guid id)
    {
        var result = await _mediator.Send(new GetDeviceInspectionsQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Dodanie wpisu o przeglądzie</summary>
    [HttpPost("{id:guid}/inspections")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> CreateInspection(
        Guid id,
        [FromBody] CreateInspectionCommand command,
        [FromServices] IValidator<CreateInspectionCommand> validator)
    {
        if (id != command.DeviceId)
            return BadRequest(new { error = "DeviceId w URL nie zgadza się z DeviceId w body." });

        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? Created($"/api/v1/devices/{id}/inspections", new { id = result.Value })
            : NotFound(new { error = result.Error });
    }

    /// <summary>Historia awarii urządzenia</summary>
    [HttpGet("{id:guid}/failures")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> GetFailures(Guid id)
    {
        var result = await _mediator.Send(new GetDeviceFailuresQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Import urządzeń z pliku Excel (.xlsx)</summary>
    [HttpPost("import")]
    [Authorize(Roles = "Administrator")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Plik jest wymagany." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Akceptowany format: .xlsx" });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new ImportDevicesCommand(stream, file.FileName));

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record ChangeStatusRequest(DeviceStatus Status);
