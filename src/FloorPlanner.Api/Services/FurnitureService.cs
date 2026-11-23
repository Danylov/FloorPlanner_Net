using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using FloorPlanner.Core.Placement;

namespace FloorPlanner.Api.Services;

// Why the furniture has a service, while floor plans and rooms do not:
//
// A service is needed where there is a real scenario of several steps. Floor plans and rooms only have simple CRUD:
// every action is a single call to the database, and the values are checked by the domain classes
// (FloorPlan.Rename, Room.Update) and by the attributes of the request. So their controllers use the repositories directly.
//
// Furniture is different. Creating or updating an item is a scenario:
// 1. load the room together with its objects;
// 2. create the item or change its values;
// 3. check the placement rules (PlacementValidator);
// 4. on a failure throw PlacementException (and, for an update, restore the old values);
// 5. otherwise save the changes.
// Such a sequence does not belong in an endpoint handler, so it is placed in this service.
//
// The price is that the layers are not uniform: furniture has three layers, floor plans and rooms have two.
// For simple CRUD this is acceptable (no extra layer without a reason).
//
// How this service relates to FurnitureEndpoints: every public method here is called by the corresponding handler
// in Endpoints/FurnitureEndpoints.cs. The handler receives the HTTP request, converts it to the values the service needs
// (FurnitureSpec), calls the service, and converts the result to a DTO and an HTTP response.
// The methods are not duplicates: the handlers know about HTTP, this service does not.
//
//   FurnitureEndpoints.GetAllAsync   -> FurnitureService.ListAsync
//   FurnitureEndpoints.GetByIdAsync  -> FurnitureService.GetAsync
//   FurnitureEndpoints.CreateAsync   -> FurnitureService.CreateAsync
//   FurnitureEndpoints.UpdateAsync   -> FurnitureService.UpdateAsync
//   FurnitureEndpoints.DeleteAsync   -> FurnitureService.DeleteAsync

/// <summary>Creates, reads, updates and deletes the furniture of a room and checks the placement rules.</summary>
public sealed class FurnitureService(
    IRepository<Room> rooms,
    IRepository<PlanObject> objects,
    IPlacementValidator validator) : IFurnitureService
{
    /// <summary>Adds a new piece of furniture to the room if the placement rules allow it (inside the room, no overlaps); otherwise throws <see cref="PlacementException"/>.</summary>
    public async Task<FurnitureItem> CreateAsync(int roomId, FurnitureSpec spec, CancellationToken ct = default)
    {
        Room room = await GetRoomAsync(roomId, ct);

        FurnitureItem item = new(spec.Name, spec.Category, spec.X, spec.Z, spec.Width, spec.Depth, spec.Height);

        // The item is validated BEFORE it is added to room.Objects.
        PlacementResult result = validator.Validate(room, item);

        if (!result.IsValid)
        {
            throw new PlacementException(result);
        }

        room.Objects.Add(item);

        await rooms.SaveChangesAsync(ct);

        return item;
    }

    /// <summary>Returns all the furniture of the room (doors and windows are not included). It serves the "list furniture of a room" request: <c>GET api/rooms/{roomId}/furniture</c>.</summary>
    public async Task<IReadOnlyList<FurnitureItem>> ListAsync(int roomId, CancellationToken ct = default)
    {
        Room room = await GetRoomAsync(roomId, ct);

        return room.Objects.OfType<FurnitureItem>().ToList();
    }

    /// <summary>Returns one piece of furniture of the room by its id; throws <see cref="NotFoundException"/> if the room or the item does not exist.</summary>
    public async Task<FurnitureItem> GetAsync(int roomId, int id, CancellationToken ct = default)
    {
        Room room = await GetRoomAsync(roomId, ct);

        return FindFurniture(room, id);
    }

    /// <summary>Replaces the values of a piece of furniture if the new placement is allowed; otherwise restores the old values and throws <see cref="PlacementException"/>.</summary>
    public async Task<FurnitureItem> UpdateAsync(int roomId, int id, FurnitureSpec spec, CancellationToken ct = default)
    {
        Room room = await GetRoomAsync(roomId, ct);
        FurnitureItem item = FindFurniture(room, id);

        string oldName = item.Name;
        string oldCategory = item.Category;
        double oldX = item.X;
        double oldZ = item.Z;
        double oldWidth = item.Width;
        double oldDepth = item.Depth;
        double oldHeight = item.Height;

        item.Update(spec.Name, spec.Category, spec.X, spec.Z, spec.Width, spec.Depth, spec.Height);

        PlacementResult result = validator.Validate(room, item);

        if (!result.IsValid)
        {
            // Return the old values, so that the rejected change is not kept in the tracked entity; nothing is saved.
            item.Update(oldName, oldCategory, oldX, oldZ, oldWidth, oldDepth, oldHeight);

            throw new PlacementException(result);
        }

        await rooms.SaveChangesAsync(ct);

        return item;
    }

    /// <summary>Deletes a piece of furniture from the room; throws <see cref="NotFoundException"/> if the room or the item does not exist.</summary>
    public async Task DeleteAsync(int roomId, int id, CancellationToken ct = default)
    {
        Room room = await GetRoomAsync(roomId, ct);
        FurnitureItem item = FindFurniture(room, id);

        room.Objects.Remove(item);
        objects.Remove(item);

        await rooms.SaveChangesAsync(ct);
    }

    /// <summary>Loads the room together with its objects; throws <see cref="NotFoundException"/> if the room does not exist.</summary>
    private async Task<Room> GetRoomAsync(int roomId, CancellationToken ct)
    {
        // Eager loading: the objects of the room are loaded together with the room.
        return await rooms.GetByIdAsync(roomId, [r => r.Objects], ct)
            ?? throw new NotFoundException("Room", roomId);
    }

    /// <summary>Finds a piece of furniture among the objects of an already loaded room; throws <see cref="NotFoundException"/> if there is none.</summary>
    private static FurnitureItem FindFurniture(Room room, int id)
    {
        return room.Objects.OfType<FurnitureItem>().FirstOrDefault(f => f.Id == id)
            ?? throw new NotFoundException("Furniture", id);
    }
}
