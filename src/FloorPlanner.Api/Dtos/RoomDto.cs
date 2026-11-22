namespace FloorPlanner.Api.Dtos;

/// <summary>A room as returned by the API, including its calculated floor area.</summary>
/// <param name="Id">The identifier of the room in the database.</param>
/// <param name="FloorPlanId">The identifier of the floor plan that the room belongs to.</param>
/// <param name="Name">The name of the room.</param>
/// <param name="Height">The height of the room, in meters.</param>
/// <param name="Corners">The corners of the floor polygon in order of traversal; they define the shape and the size of the room.</param>
/// <param name="Area">The floor area in square meters, calculated from the corners (it is not stored in the database).</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record RoomDto(int Id, int FloorPlanId, string Name, double Height, List<PointDto> Corners, double Area);
// ReSharper restore NotAccessedPositionalProperty.Global
