using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trisecmed.Infrastructure.Data;
using Trisecmed.Domain.Entities;


namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly TrisecmedDbContext _db;

    public DevicesController(TrisecmedDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var devices = await _db.MedicalDevices.ToListAsync();
        return Ok(devices);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MedicalDevice device)
    {
        device.Id = Guid.NewGuid();
        _db.MedicalDevices.Add(device);
        await _db.SaveChangesAsync();
        return Ok(device);
    }
}