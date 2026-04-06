using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Trisecmed.API.Middleware;
using Trisecmed.API.Services;
using Trisecmed.Application;
using Trisecmed.Infrastructure;
using Trisecmed.Infrastructure.Data;
using Trisecmed.Infrastructure.Jobs;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/trisecmed-.log", rollingInterval: RollingInterval.Day));

    // Clean Architecture DI
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();

    // Controllers + OpenAPI
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Default")!, name: "postgresql");

    // Hangfire
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default")!)));
    builder.Services.AddHangfireServer();

    // CORS for dev
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    // Global exception handler — first in pipeline
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Auto-migrate in Development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrisecmedDbContext>();
        await db.Database.MigrateAsync();
        await SeedData.InitializeAsync(app.Services);
    }

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    // OpenAPI doc at /openapi/v1.json + Scalar UI at /scalar/v1
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Hangfire dashboard (admin only in production)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()],
    });

    // Recurring Hangfire jobs
    RecurringJob.AddOrUpdate<InspectionDueNotificationJob>(
        "inspection-due-notification",
        job => job.ExecuteAsync(),
        "0 7 * * *"); // codziennie o 7:00

    RecurringJob.AddOrUpdate<WarrantyExpirationJob>(
        "warranty-expiration",
        job => job.ExecuteAsync(),
        "0 8 * * *"); // codziennie o 8:00

    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("Trisecmed API starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "default");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Umożliwia WebApplicationFactory<Program> w testach integracyjnych
public partial class Program { }
