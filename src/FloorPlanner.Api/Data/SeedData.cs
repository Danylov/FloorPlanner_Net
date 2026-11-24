using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Data;

/// <summary>Fills an empty database with the demo data.</summary>
public static class SeedData
{
    /// <summary>Adds the demo plan to the database if it contains no floor plans yet.</summary>
    /// <param name="db">The database context to fill.</param>
    /// <param name="ct">A token that cancels the database operations.</param>
    public static async Task InitializeAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.FloorPlans.AnyAsync(ct))
        {
            return;
        }

        db.FloorPlans.Add(DemoPlan.Build());

        await db.SaveChangesAsync(ct);
    }
}
