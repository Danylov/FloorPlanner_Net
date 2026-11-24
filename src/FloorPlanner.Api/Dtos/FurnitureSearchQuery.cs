using System.ComponentModel.DataAnnotations;

namespace FloorPlanner.Api.Dtos;

/// <summary>The query-string parameters of the furniture search.</summary>
/// <param name="Category">The category to search for; the case is ignored. If it is empty, all the categories are returned.</param>
/// <param name="Sort">The sort order: name, -name, area or -area (the minus sign means descending order).</param>
/// <param name="Page">The number of the page, starting from 1.</param>
/// <param name="PageSize">The number of items on a page (from 1 to 50).</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record FurnitureSearchQuery(
    string? Category = null,
    [RegularExpression("^-?(name|area)$")] string Sort = "name",
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 50)] int PageSize = 10);
// ReSharper restore NotAccessedPositionalProperty.Global
