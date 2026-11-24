using FloorPlanner.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Controllers;

/// <summary>Reports that are built with a raw SQL query.</summary>
/// <param name="db">The database context: the report query is executed through it.</param>
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(AppDbContext db) : ControllerBase
{
    [HttpGet("room-footprint")]
    public async Task<ActionResult<List<RoomFootprintRow>>> GetRoomFootprint(CancellationToken cancellationToken)
    {
        // The text of the query is read from the file that is copied next to the application (sql/report.sql).
        string sql = await System.IO.File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "sql", "report.sql"), cancellationToken);

        // Why this is safe: the query is a fixed text from our own file. It has no parameters, and nothing that comes
        // from the client (no strings, no numbers) is concatenated into it, so there is no way to inject foreign SQL code.
        List<RoomFootprintRow> rows = await db.Set<RoomFootprintRow>()
            .FromSqlRaw(sql)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }
}
