namespace FloorPlanner.Api.Dtos;

/// <summary>
/// One entry of the five largest furniture items list in a floor plan (<c>PlanSummaryDto.Top5Largest</c>);
/// the list has up to five such entries, one for each item. It is returned by the API (a data transfer object).
/// </summary>
/// <param name="Id">The identifier of the furniture item in the database.</param>
/// <param name="Name">The name of the item.</param>
/// <param name="Category">The category of the item.</param>
/// <param name="FootprintArea">The area of the item's footprint (width times depth), in square meters (rounded to 2 decimals).</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record LargestItemDto(int Id, string Name, string Category, double FootprintArea);
// ReSharper restore NotAccessedPositionalProperty.Global
