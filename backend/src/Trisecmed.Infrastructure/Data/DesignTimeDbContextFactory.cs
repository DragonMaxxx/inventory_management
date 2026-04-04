using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Trisecmed.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrisecmedDbContext>
{
    public TrisecmedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrisecmedDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=trisecmed;Username=trisecmed_user;Password=dev_password_123");
        return new TrisecmedDbContext(optionsBuilder.Options);
    }
}
