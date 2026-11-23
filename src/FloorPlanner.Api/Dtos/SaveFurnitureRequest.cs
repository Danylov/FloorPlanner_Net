using System.ComponentModel.DataAnnotations;

namespace FloorPlanner.Api.Dtos;

/// <summary>The request body for creating or updating a piece of furniture.</summary>
/// <param name="Name">The name of the item (required, up to 100 characters).</param>
/// <param name="Category">The category of the item, for example Chair or Table (required, up to 50 characters).</param>
/// <param name="X">The X coordinate of the center of the item's base, in meters (from -1000 to 1000).</param>
/// <param name="Z">The Z coordinate of the center of the item's base, in meters (from -1000 to 1000).</param>
/// <param name="Width">The size of the item along the X axis, in meters (from 0.05 to 10).</param>
/// <param name="Depth">The size of the item along the Z axis, in meters (from 0.05 to 10).</param>
/// <param name="Height">The height of the item, in meters (from 0.05 to 10).</param>
public sealed record SaveFurnitureRequest(
    [Required, StringLength(100)] string Name,
    [Required, StringLength(50)] string Category,
    [Range(-1000, 1000)] double X,
    [Range(-1000, 1000)] double Z,
    [Range(0.05, 10)] double Width,
    [Range(0.05, 10)] double Depth,
    [Range(0.05, 10)] double Height);
