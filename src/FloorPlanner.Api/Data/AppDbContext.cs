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

        // The row of the SQL report. This line does NOT create a table in the database; it says the opposite:
        // - Entity<RoomFootprintRow>() makes the type a part of the model, so that EF Core can create such objects
        //   from the result of the raw SQL query (db.Set<RoomFootprintRow>().FromSqlRaw(...));
        // - HasNoKey() means that the entity has no primary key (the rows of the report have no identifier);
        // - ToTable((string?)null) means that the entity is not mapped to any table (or view), so no migration is created for it.
        // The entity is only a "shape" for the result of the query: its columns come from sql/report.sql, and nothing is stored.
        // (ToView(null) does not work for this: in EF Core 11 it still creates a table in the migration.)
        modelBuilder.Entity<RoomFootprintRow>().HasNoKey().ToTable((string?)null);
    }
}
