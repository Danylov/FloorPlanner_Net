using System.ComponentModel.DataAnnotations;

namespace FloorPlanner.Api.Data;

// Where and when the RoomFootprintRow objects are created:
//
// Our code never calls "new RoomFootprintRow(...)". The objects are created by EF Core while it executes the query.
//
// Where: in ReportsController.GetRoomFootprint, in the chain
//   db.Set<RoomFootprintRow>().FromSqlRaw(sql).AsNoTracking().ToListAsync(cancellationToken)
//
// When: on every request GET api/reports/room-footprint, at the moment ToListAsync is executed.
// Before that the query is only described, and nothing is sent to the database.
//
// Order of actions:
// 1. ToListAsync sends the SQL from sql/report.sql to PostgreSQL.
// 2. The database returns a table of the result with the columns plan_name, room_name, furniture_count,
//    total_footprint and rank_in_plan (one row for each room).
// 3. For every row EF Core creates one RoomFootprintRow object and writes the values of the columns to its properties.
//    The columns are matched with the properties by name (plan_name -> PlanName, and so on): this is done by
//    UseSnakeCaseNamingConvention(), and the aliases (AS plan_name, ...) in the SQL are chosen for that.
//    The properties are init-only because EF Core fills them once, when the object is created.
// 4. The objects are collected into a List<RoomFootprintRow>; the controller returns it with Ok(rows),
//    and ASP.NET Core converts it to JSON.
//
// AsNoTracking means that EF Core does not track the objects after it has created them (the query only reads data).
// When the response is sent, the objects are not needed any more and are removed by the garbage collector.

/// <summary>A row of the room footprint report: the furniture of a room and the rank of the room in its plan.</summary>
public sealed class RoomFootprintRow
{
    /// <summary>Gets the name of the floor plan.</summary>
    [MaxLength(100)] // The same limit as the name column of the floor_plans table.
    public string PlanName { get; init; } = string.Empty;

    /// <summary>Gets the name of the room.</summary>
    [MaxLength(100)] // The same limit as the name column of the rooms table.
    public string RoomName { get; init; } = string.Empty;

    /// <summary>Gets the number of furniture items in the room.</summary>
    public long FurnitureCount { get; init; }

    /// <summary>Gets the total area of the footprints of the furniture in the room, in square meters (rounded to 2 decimals).</summary>
    public double TotalFootprint { get; init; }

    /// <summary>Gets the rank of the room in its plan by the total footprint (1 is the largest).</summary>
    public long RankInPlan { get; init; }
}
