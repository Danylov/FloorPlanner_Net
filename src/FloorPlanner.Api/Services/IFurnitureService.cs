using FloorPlanner.Core.Domain;

namespace FloorPlanner.Api.Services;

/// <summary>Furniture operations of a room that apply the placement rules.</summary>
public interface IFurnitureService
{
    Task<FurnitureItem> CreateAsync(int roomId, FurnitureSpec spec, CancellationToken ct = default);

    Task<IReadOnlyList<FurnitureItem>> ListAsync(int roomId, CancellationToken ct = default);

    Task<FurnitureItem> GetAsync(int roomId, int id, CancellationToken ct = default);

    Task<FurnitureItem> UpdateAsync(int roomId, int id, FurnitureSpec spec, CancellationToken ct = default);

    Task DeleteAsync(int roomId, int id, CancellationToken ct = default);
}
