namespace FloorPlanner.Api.Dtos;

/// <summary>One page of a larger result.</summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="Items">The items of the requested page.</param>
/// <param name="Page">The number of the page, starting from 1.</param>
/// <param name="PageSize">The maximum number of items on a page.</param>
/// <param name="TotalCount">The total number of items found, across all the pages.</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
// ReSharper restore NotAccessedPositionalProperty.Global
