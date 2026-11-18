using FloorPlanner.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Data;

/// <summary>The EF Core database context of the floor planner.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) // Constructor injection (DI): the container passes the options
{
    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<PlanObject> PlanObjects => Set<PlanObject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
