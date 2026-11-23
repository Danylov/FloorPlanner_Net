// Why FurnitureSpec is needed, although its fields repeat SaveFurnitureRequest and partly FurnitureItem:
//
// 1. The service cannot take a FurnitureItem. It is a database entity: it has Id and RoomId, and its constructor
//    validates the values at once. When a piece of furniture is updated, the client sends only the new values
//    for an item that already exists, so a set of plain values is enough and a second entity object is not needed.
//
// 2. The service must not depend on the web layer. SaveFurnitureRequest is an HTTP request body with validation
//    attributes ([Range], [Required]). If the service accepted it, the service would depend on the API format.
//    With FurnitureSpec the service does not know where the data comes from: a controller, a test or demo data.
//
// 3. A short signature. CreateAsync(roomId, spec, ct) replaces a method with seven separate parameters
//    (name, category, x, z, width, depth, height).
//
// The price is that three types hold similar fields, so a new field must be added in all of them.

namespace FloorPlanner.Api.Services;

/// <summary>The values that describe a piece of furniture when it is created or updated.</summary>
/// <param name="Name">The name of the item.</param>
/// <param name="Category">The category of the item, for example Chair or Table.</param>
/// <param name="X">The X coordinate of the center of the item's base, in meters.</param>
/// <param name="Z">The Z coordinate of the center of the item's base, in meters.</param>
/// <param name="Width">The size of the item along the X axis, in meters.</param>
/// <param name="Depth">The size of the item along the Z axis, in meters.</param>
/// <param name="Height">The height of the item, in meters.</param>
public sealed record FurnitureSpec(string Name, string Category, double X, double Z, double Width, double Depth, double Height);
