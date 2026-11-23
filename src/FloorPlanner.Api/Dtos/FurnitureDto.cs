namespace FloorPlanner.Api.Dtos;

/// <summary>A piece of furniture as returned by the API.</summary>
/// <param name="Id">The identifier of the furniture item in the database.</param>
/// <param name="RoomId">The identifier of the room that the item stands in.</param>
/// <param name="Name">The name of the item.</param>
/// <param name="Category">The category of the item, for example Chair or Table.</param>
/// <param name="X">The X coordinate of the center of the item's base, in meters.</param>
/// <param name="Z">The Z coordinate of the center of the item's base, in meters.</param>
/// <param name="Width">The size of the item along the X axis, in meters.</param>
/// <param name="Depth">The size of the item along the Z axis, in meters.</param>
/// <param name="Height">The height of the item, in meters.</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record FurnitureDto(int Id, int RoomId, string Name, string Category, double X, double Z, double Width, double Depth, double Height);
// ReSharper restore NotAccessedPositionalProperty.Global
