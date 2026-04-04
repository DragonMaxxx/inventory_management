using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Application.Equipment.Commands;
using Trisecmed.Application.Equipment.Queries;


namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var devices = await _mediator.Send(new GetDevicesQuery());
        return Ok(devices);
    }

    [HttpPost]
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

        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { id = result.Value });
    }
}
