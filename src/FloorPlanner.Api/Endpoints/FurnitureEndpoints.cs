using FloorPlanner.Api.Dtos;
using FloorPlanner.Api.Services;
using FloorPlanner.Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FloorPlanner.Api.Endpoints;

/// <summary>The Minimal API endpoints for the furniture of a room.</summary>
public static class FurnitureEndpoints
{
    public static void MapFurnitureEndpoints(this IEndpointRouteBuilder app)
    {
        // Creates a group of endpoints with the common URL prefix (roomId must be an integer), so the prefix is not repeated
        // in every route; the "Furniture" tag groups these endpoints together in Swagger.
        RouteGroupBuilder group = app.MapGroup("api/rooms/{roomId:int}/furniture").WithTags("Furniture");

        group.MapGet(string.Empty, GetAllAsync);
        // WithName gives this route the name "GetFurniture" (it is not part of the URL). CreateAsync uses the name
        // in CreatedAtRoute to build the Location header with the URL of the new item.
        group.MapGet("{id:int}", GetByIdAsync).WithName("GetFurniture");
        group.MapPost(string.Empty, CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);
    }

    // The handlers contain no try/catch: PlacementException becomes 409 and NotFoundException becomes 404
    // in ExceptionHandlingMiddleware.
    private static async Task<Ok<List<FurnitureDto>>> GetAllAsync(
        int roomId, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        IReadOnlyList<FurnitureItem> items = await furniture.ListAsync(roomId, cancellationToken);

        return TypedResults.Ok(items.Select(ToDto).ToList());
    }

    private static async Task<Ok<FurnitureDto>> GetByIdAsync(
        int roomId, int id, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        FurnitureItem item = await furniture.GetAsync(roomId, id, cancellationToken);

        return TypedResults.Ok(ToDto(item));
    }

    private static async Task<CreatedAtRoute<FurnitureDto>> CreateAsync(
        int roomId, SaveFurnitureRequest request, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        FurnitureItem item = await furniture.CreateAsync(roomId, ToSpec(request), cancellationToken);

        return TypedResults.CreatedAtRoute(ToDto(item), "GetFurniture", new { roomId, id = item.Id });
    }

    private static async Task<Ok<FurnitureDto>> UpdateAsync(
        int roomId, int id, SaveFurnitureRequest request, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        FurnitureItem item = await furniture.UpdateAsync(roomId, id, ToSpec(request), cancellationToken);

        return TypedResults.Ok(ToDto(item));
    }

    private static async Task<NoContent> DeleteAsync(
        int roomId, int id, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        await furniture.DeleteAsync(roomId, id, cancellationToken);

        return TypedResults.NoContent();
    }

    private static FurnitureSpec ToSpec(SaveFurnitureRequest request)
    {
        return new FurnitureSpec(request.Name, request.Category, request.X, request.Z, request.Width, request.Depth, request.Height);
    }

    private static FurnitureDto ToDto(FurnitureItem item)
    {
        return new FurnitureDto(
            item.Id, item.RoomId, item.Name, item.Category, item.X, item.Z, item.Width, item.Depth, item.Height);
    }
}
