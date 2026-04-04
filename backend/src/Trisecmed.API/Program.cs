using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Trisecmed.API.Services;
using Trisecmed.Application;
using Trisecmed.Infrastructure;
using Trisecmed.Infrastructure.Data;
using Microsoft.OpenApi.Models;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

<<<<<<< HEAD
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/trisecmed-.log", rollingInterval: RollingInterval.Day));
=======
// Dodaj DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Serwisy
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trisecmed API", Version = "v1" });
});
>>>>>>> 8fa7545c91d5a89ff4740c63ab57a6902f000936

    // Clean Architecture DI
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

<<<<<<< HEAD
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
=======
// Middleware Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trisecmed API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
>>>>>>> 8fa7545c91d5a89ff4740c63ab57a6902f000936

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Default")!, name: "postgresql");

    // CORS for dev
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

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
