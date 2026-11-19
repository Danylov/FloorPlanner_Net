namespace FloorPlanner.Api.Dtos;

/// <summary>
/// A floor plan as returned by the API (a data transfer object).
/// It gives the client only the fields it needs (including the room count instead of the whole room list)
/// and keeps the API format independent of the domain class <c>FloorPlan</c>.
/// </summary>
// The properties are read by the JSON serializer through reflection, not from our code.
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record FloorPlanDto(int Id, string Name, DateTime CreatedAt, int RoomCount);
// ReSharper restore NotAccessedPositionalProperty.Global
