using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IRepository<Category> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items.Select(c => new { c.Id, c.Name, c.Description, c.CreatedAt }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return NotFound(new { error = "Kategoria nie została znaleziona." });
        return Ok(new { cat.Id, cat.Name, cat.Description, cat.CreatedAt });
    }

    [HttpPost]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        var cat = new Category { Name = request.Name, Description = request.Description };
        await _repo.AddAsync(cat);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, new { id = cat.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EquipmentManager,Administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return NotFound(new { error = "Kategoria nie została znaleziona." });

        cat.Name = request.Name;
        cat.Description = request.Description;
        _repo.Update(cat);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return NotFound(new { error = "Kategoria nie została znaleziona." });

        _repo.Remove(cat);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

public record CategoryRequest(string Name, string? Description = null);
