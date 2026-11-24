namespace FloorPlanner.Api.Dtos;

/// <summary>A piece of furniture found by the search, with the room and the plan it belongs to.</summary>
/// <param name="Id">The identifier of the furniture item in the database.</param>
/// <param name="RoomId">The identifier of the room that the item stands in.</param>
/// <param name="Name">The name of the item.</param>
/// <param name="Category">The category of the item, for example Chair or Table.</param>
/// <param name="Width">The size of the item along the X axis, in meters.</param>
/// <param name="Depth">The size of the item along the Z axis, in meters.</param>
/// <param name="Area">The area of the item's footprint (width times depth), in square meters.</param>
/// <param name="RoomName">The name of the room that the item stands in.</param>
/// <param name="PlanId">The identifier of the floor plan that the room belongs to.</param>
/// <param name="PlanName">The name of the floor plan that the room belongs to.</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record FurnitureSearchItemDto(
    int Id, int RoomId, string Name, string Category, double Width, double Depth, double Area, string RoomName, int PlanId, string PlanName);
// ReSharper restore NotAccessedPositionalProperty.Global
