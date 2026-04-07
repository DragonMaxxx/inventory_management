using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IRepository<Department> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentsController(IRepository<Department> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items.Select(d => new { d.Id, d.Name, d.Code, d.CreatedAt }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dept = await _repo.GetByIdAsync(id);
        if (dept is null) return NotFound(new { error = "Oddział nie został znaleziony." });
        return Ok(new { dept.Id, dept.Name, dept.Code, dept.CreatedAt });
    }

    [HttpPost]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Create([FromBody] DepartmentRequest request)
    {
        var dept = new Department { Name = request.Name, Code = request.Code };
        await _repo.AddAsync(dept);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, new { id = dept.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentRequest request)
    {
        var dept = await _repo.GetByIdAsync(id);
        if (dept is null) return NotFound(new { error = "Oddział nie został znaleziony." });

        dept.Name = request.Name;
        dept.Code = request.Code;
        _repo.Update(dept);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dept = await _repo.GetByIdAsync(id);
        if (dept is null) return NotFound(new { error = "Oddział nie został znaleziony." });

        _repo.Remove(dept);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

public record DepartmentRequest(string Name, string? Code = null);
