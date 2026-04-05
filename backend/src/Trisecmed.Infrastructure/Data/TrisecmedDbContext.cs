using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Infrastructure.Data;

public class TrisecmedDbContext : DbContext
{
    public TrisecmedDbContext(DbContextOptions<TrisecmedDbContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Failure> Failures => Set<Failure>();
    public DbSet<FailureStatusHistory> FailureStatusHistories => Set<FailureStatusHistory>();
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<ServiceContract> ServiceContracts => Set<ServiceContract>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Device
        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(255).IsRequired();
            e.Property(d => d.InventoryNumber).HasMaxLength(100).IsRequired();
            e.HasIndex(d => d.InventoryNumber).IsUnique();
            e.Property(d => d.SerialNumber).HasMaxLength(100);
            e.Property(d => d.Manufacturer).HasMaxLength(255).IsRequired();
            e.Property(d => d.Model).HasMaxLength(255).IsRequired();
            e.Property(d => d.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(d => d.PurchasePrice).HasPrecision(12, 2);
            e.HasIndex(d => d.NextInspectionDate);
            e.HasOne(d => d.Category).WithMany(c => c.Devices).HasForeignKey(d => d.CategoryId);
            e.HasOne(d => d.Department).WithMany(dep => dep.Devices).HasForeignKey(d => d.DepartmentId);
            e.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // Department
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(255).IsRequired();
            e.Property(d => d.Code).HasMaxLength(50);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(255).IsRequired();
        });

        // Inspection
        modelBuilder.Entity<Inspection>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasOne(i => i.Device).WithMany(d => d.Inspections).HasForeignKey(i => i.DeviceId);
            e.HasOne(i => i.CreatedByUser).WithMany().HasForeignKey(i => i.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(i => i.PerformedBy).HasMaxLength(255);
        });

        // Failure
        modelBuilder.Entity<Failure>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Description).IsRequired();
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(f => f.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(f => f.RepairCost).HasPrecision(12, 2);
            e.HasOne(f => f.Device).WithMany(d => d.Failures).HasForeignKey(f => f.DeviceId);
            e.HasOne(f => f.ReportedByUser).WithMany().HasForeignKey(f => f.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Department).WithMany().HasForeignKey(f => f.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.ServiceProvider).WithMany(sp => sp.Failures).HasForeignKey(f => f.ServiceProviderId);
        });

        // FailureStatusHistory
        modelBuilder.Entity<FailureStatusHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.OldStatus).HasConversion<string>().HasMaxLength(30);
            e.Property(h => h.NewStatus).HasConversion<string>().HasMaxLength(30);
            e.HasOne(h => h.Failure).WithMany(f => f.StatusHistory).HasForeignKey(h => h.FailureId);
            e.HasOne(h => h.ChangedByUser).WithMany().HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ServiceProvider
        modelBuilder.Entity<ServiceProvider>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.Property(sp => sp.Name).HasMaxLength(255).IsRequired();
            e.Property(sp => sp.Email).HasMaxLength(255);
            e.Property(sp => sp.Phone).HasMaxLength(50);
            e.Property(sp => sp.TaxId).HasMaxLength(50);
        });

        // ServiceContract
        modelBuilder.Entity<ServiceContract>(e =>
        {
            e.HasKey(sc => sc.Id);
            e.Property(sc => sc.ContractNumber).HasMaxLength(100).IsRequired();
            e.Property(sc => sc.Value).HasPrecision(12, 2);
            e.HasOne(sc => sc.ServiceProvider).WithMany(sp => sp.ServiceContracts).HasForeignKey(sc => sc.ServiceProviderId);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.Property(u => u.ActivationToken).HasMaxLength(500);
            e.Property(u => u.RefreshToken).HasMaxLength(500);
            e.HasOne(u => u.Department).WithMany(d => d.Users).HasForeignKey(u => u.DepartmentId);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            e.Property(a => a.IpAddress).HasMaxLength(45);
            e.HasIndex(a => a.CreatedAt);
            e.HasIndex(a => new { a.EntityType, a.EntityId });
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Type).HasMaxLength(100).IsRequired();
            e.Property(n => n.Subject).HasMaxLength(500).IsRequired();
            e.Property(n => n.RecipientEmail).HasMaxLength(255).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
