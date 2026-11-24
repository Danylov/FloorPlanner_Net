namespace FloorPlanner.Api.Dtos;

/// <summary>
/// The summary of a floor plan (the rooms, the furniture and the statistics by category), as returned by the API (a data transfer object).
/// </summary>
/// <param name="PlanId">The identifier of the floor plan.</param>
/// <param name="Name">The name of the floor plan.</param>
/// <param name="RoomCount">The number of rooms in the plan.</param>
/// <param name="TotalRoomArea">The total floor area of all the rooms, in square meters (rounded to 2 decimals).</param>
/// <param name="FurnitureCount">The number of furniture items in all the rooms (doors and windows are not counted).</param>
/// <param name="HasEmptyRooms">Whether the plan has at least one room without furniture.</param>
/// <param name="Categories">The statistics by category, from the most numerous category to the least numerous one.</param>
/// <param name="Top5Largest">The five furniture items with the largest footprint, the largest first.</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record PlanSummaryDto(
    int PlanId,
    string Name,
    int RoomCount,
    double TotalRoomArea,
    int FurnitureCount,
    bool HasEmptyRooms,
    List<CategoryStatDto> Categories,
    List<LargestItemDto> Top5Largest);
// ReSharper restore NotAccessedPositionalProperty.Global
