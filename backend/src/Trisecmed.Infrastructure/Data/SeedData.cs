using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Infrastructure.Data;

public static class SeedData
{
    // Stałe GUID-y żeby były przewidywalne w dev
    public static readonly Guid DefaultDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DefaultUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid WorkerUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ManagerUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrisecmedDbContext>();

        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Id = DefaultDepartmentId, Name = "Oddział Chirurgii", Code = "CHIR" },
                new Department { Name = "Oddział Kardiologii", Code = "KARD" },
                new Department { Name = "Oddział Intensywnej Terapii", Code = "OIT" }
            );
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Id = DefaultCategoryId, Name = "Aparatura diagnostyczna" },
                new Category { Name = "Aparatura terapeutyczna" },
                new Category { Name = "Sprzęt laboratoryjny" },
                new Category { Name = "Wyposażenie pomocnicze" }
            );
        }

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    Id = DefaultUserId,
                    Email = "admin@trisecmed.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123", 12),
                    FirstName = "Admin",
                    LastName = "System",
                    Role = UserRole.Administrator,
                    IsActive = true
                },
                new User
                {
                    Id = WorkerUserId,
                    Email = "worker@trisecmed.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker123", 12),
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Role = UserRole.EquipmentWorker,
                    DepartmentId = DefaultDepartmentId,
                    IsActive = true
                },
                new User
                {
                    Id = ManagerUserId,
                    Email = "manager@trisecmed.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123", 12),
                    FirstName = "Anna",
                    LastName = "Nowak",
                    Role = UserRole.EquipmentManager,
                    DepartmentId = DefaultDepartmentId,
                    IsActive = true
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
