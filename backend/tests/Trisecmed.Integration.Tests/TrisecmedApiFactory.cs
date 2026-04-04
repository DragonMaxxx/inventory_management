using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Integration.Tests;

public class TrisecmedApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Zastąp PostgreSQL in-memory database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TrisecmedDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Usuń też interceptor, który wymaga ICurrentUserService
            var interceptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AuditInterceptor));
            if (interceptor != null)
                services.Remove(interceptor);

            services.AddDbContext<TrisecmedDbContext>(options =>
                options.UseInMemoryDatabase("TrisecmedTestDb_" + Guid.NewGuid()));
        });
    }
}
