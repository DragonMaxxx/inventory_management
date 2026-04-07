using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Domain.Interfaces;
using ServiceProviderEntity = Trisecmed.Domain.Entities.ServiceProvider;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/service-providers")]
[Authorize(Roles = "EquipmentWorker,EquipmentManager,Administrator")]
public class ServiceProvidersController : ControllerBase
{
    private readonly IRepository<ServiceProviderEntity> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceProvidersController(IRepository<ServiceProviderEntity> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items.Select(sp => new
        {
            sp.Id, sp.Name, sp.ContactPerson, sp.Email, sp.Phone, sp.Address, sp.TaxId, sp.Notes, sp.CreatedAt
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sp = await _repo.GetByIdAsync(id);
        if (sp is null) return NotFound(new { error = "Serwisant nie został znaleziony." });
        return Ok(new { sp.Id, sp.Name, sp.ContactPerson, sp.Email, sp.Phone, sp.Address, sp.TaxId, sp.Notes, sp.CreatedAt });
    }

    [HttpPost]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateServiceProviderRequest request)
    {
        var sp = new ServiceProviderEntity
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxId = request.TaxId,
            Notes = request.Notes,
        };

        await _repo.AddAsync(sp);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = sp.Id }, new { id = sp.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateServiceProviderRequest request)
    {
        var sp = await _repo.GetByIdAsync(id);
        if (sp is null) return NotFound(new { error = "Serwisant nie został znaleziony." });

        sp.Name = request.Name;
        sp.ContactPerson = request.ContactPerson;
        sp.Email = request.Email;
        sp.Phone = request.Phone;
        sp.Address = request.Address;
        sp.TaxId = request.TaxId;
        sp.Notes = request.Notes;

        _repo.Update(sp);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sp = await _repo.GetByIdAsync(id);
        if (sp is null) return NotFound(new { error = "Serwisant nie został znaleziony." });

        _repo.Remove(sp);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateServiceProviderRequest(
    string Name,
    string? ContactPerson = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? TaxId = null,
    string? Notes = null);
