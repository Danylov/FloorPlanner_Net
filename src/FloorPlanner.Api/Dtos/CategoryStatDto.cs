namespace FloorPlanner.Api.Dtos;

/// <summary>The statistics of one furniture category in a floor plan, as returned by the API (a data transfer object).</summary>
/// <param name="Category">The name of the category, for example Chair or Table.</param>
/// <param name="Count">The number of furniture items of this category.</param>
/// <param name="TotalFootprintArea">The total area of the footprints of these items, in square meters (rounded to 2 decimals).</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record CategoryStatDto(string Category, int Count, double TotalFootprintArea);
// ReSharper restore NotAccessedPositionalProperty.Global
