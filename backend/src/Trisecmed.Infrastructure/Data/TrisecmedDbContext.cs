using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;

namespace Trisecmed.Infrastructure.Data;

public class TrisecmedDbContext : DbContext
{
    public TrisecmedDbContext(DbContextOptions<TrisecmedDbContext> options)
        : base(options) { }

    public DbSet<MedicalDevice> MedicalDevices => Set<MedicalDevice>();
}