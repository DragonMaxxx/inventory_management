using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;

namespace Trisecmed.Infrastructure;

public class TrisecmedDbContext : DbContext
{
    public TrisecmedDbContext(DbContextOptions<TrisecmedDbContext> options)
        : base(options) { }

    public DbSet<MedicalDevice> MedicalDevices => Set<MedicalDevice>();
    public DbSet<User> Users => Set<User>();
}