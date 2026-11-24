using FloorPlanner.Api.Dtos;
using FloorPlanner.Api.Services;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FloorPlanner.Api.Endpoints;

/// <summary>The Minimal API endpoints that search the furniture and list the furniture of a floor plan.</summary>
public static class FurnitureSearchEndpoints
{
    public static void MapFurnitureSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/furniture", SearchAsync).WithTags("Furniture");
        app.MapGet("api/floorplans/{planId:int}/furniture", ListByPlanAsync).WithTags("Furniture");
    }

    private static async Task<Ok<PagedResult<FurnitureSearchItemDto>>> SearchAsync(
        [AsParameters] FurnitureSearchQuery query, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        // The service returns one page of the found items together with the paging data (the page number, the page size
        // and the total count). The variable has the type PagedResult<FurnitureSearchItemDto>, because this is exactly
        // what the client receives in the body of the response; the handler only passes the result on, it does not change it.
        PagedResult<FurnitureSearchItemDto> result = await furniture.SearchAsync(query, cancellationToken);

        // TypedResults.Ok creates the response 200 OK with the result as the body. Unlike Results.Ok, it keeps the exact type
        // of the response (Ok<PagedResult<FurnitureSearchItemDto>>), which is why this type is written in the return type of
        // the method: Swagger reads it and shows the status 200 and the structure of the body in the documentation.
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<FurnitureSearchItemDto>>> ListByPlanAsync(
        int planId, IRepository<FloorPlan> plans, IFurnitureService furniture, CancellationToken cancellationToken)
    {
        if (await plans.GetByIdAsync(planId, cancellationToken) is null)
        {
            throw new NotFoundException("FloorPlan", planId);
        }

        // The service returns the whole list of the furniture of the plan (from all its rooms). There is no paging here, so the result
        // is a plain list IReadOnlyList<FurnitureSearchItemDto>, not PagedResult: the client receives all the items at once.
        // TypedResults.Ok creates the response 200 OK with this list as the body and keeps the exact type of the response
        // (Ok<IReadOnlyList<FurnitureSearchItemDto>>), which is written in the return type of the method, so that Swagger
        // shows the status 200 and the structure of the body. If the plan does not exist, the check above has already thrown
        // NotFoundException, which ExceptionHandlingMiddleware turns into 404.
        return TypedResults.Ok(await furniture.ListByPlanAsync(planId, cancellationToken));
    }
}
