using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;

namespace Trisecmed.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MedicalDevice> MedicalDevices { get; set; }
    }
}