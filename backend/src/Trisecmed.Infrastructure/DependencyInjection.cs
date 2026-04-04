using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trisecmed.Application.Identity.Interfaces;
using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Auth;
using Trisecmed.Infrastructure.Data;
using Trisecmed.Infrastructure.Repositories;

namespace Trisecmed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<TrisecmedDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IFailureRepository, FailureRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Auth services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<AuditInterceptor>();

        return services;
    }
}
