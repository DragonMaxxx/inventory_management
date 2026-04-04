using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trisecmed.Infrastructure.Data;
using Trisecmed.Domain.Entities;

namespace Trisecmed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalDevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalDevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalDevices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalDevice>>> GetDevices()
        {
            return await _context.MedicalDevices.ToListAsync();
        }

        // GET: api/MedicalDevices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalDevice>> GetDevice(Guid id)
        {
            var device = await _context.MedicalDevices.FindAsync(id);
            if (device == null) return NotFound();
            return device;
        }

        // POST: api/MedicalDevices
        [HttpPost]
        public async Task<ActionResult<MedicalDevice>> CreateDevice(MedicalDevice device)
        {
            _context.MedicalDevices.Add(device);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }

        // PUT: api/MedicalDevices/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(Guid id, MedicalDevice device)
        {
            if (id != device.Id) return BadRequest();
            _context.Entry(device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/MedicalDevices/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            var device = await _context.MedicalDevices.FindAsync(id);
            if (device == null) return NotFound();

            _context.MedicalDevices.Remove(device);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool DeviceExists(Guid id)
        {
            return _context.MedicalDevices.Any(e => e.Id == id);
        }
    }
}