using FloorPlanner.Api.Data;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Placement;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()); // IoC container setup: dependency registration, Scoped

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>)); // IoC container setup: dependency registration, Scoped: interface -> implementation
builder.Services.AddSingleton<IPlacementValidator, PlacementValidator>(); // IoC container setup: dependency registration, Singleton: no state

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program { }
