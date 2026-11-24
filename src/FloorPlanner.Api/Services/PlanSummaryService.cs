using FloorPlanner.Api.Dtos;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Services;

/// <summary>Builds the summary of a floor plan with LINQ.</summary>
public sealed class PlanSummaryService(IRepository<FloorPlan> plans) : IPlanSummaryService
{
    public async Task<PlanSummaryDto> GetAsync(int planId, CancellationToken ct = default)
    {
        // Eager loading: the plan is loaded together with its rooms and their objects in one query.
        // AsNoTracking: the data is only read, so EF does not need to track the changes of the entities.
        FloorPlan plan = await plans.Query()
            .AsNoTracking()
            .Include(p => p.Rooms)
            .ThenInclude(r => r.Objects)
            .FirstOrDefaultAsync(p => p.Id == planId, ct)
            ?? throw new NotFoundException("FloorPlan", planId);

        // Everything below is calculated in memory, not in SQL: RoomCalculator is our own C# code and cannot be translated to SQL.
        List<Room> rooms = plan.Rooms;

        // All the furniture of the plan in one flat list: SelectMany joins the objects of all the rooms,
        // and OfType keeps only the furniture (the doors and the windows are dropped).
        List<FurnitureItem> furniture = rooms
            .SelectMany(r => r.Objects)
            .OfType<FurnitureItem>()
            .ToList();

        // The total floor area of the plan: the areas of all the rooms (calculated from their corners) are added up.
        double totalRoomArea = rooms.Sum(r => RoomCalculator.Area(r));

        // Whether the plan has at least one room without furniture: Any returns true as soon as it finds such a room
        // (a room where there is no furniture item; the doors and the windows do not count).
        bool hasEmptyRooms = rooms.Any(r => !r.Objects.OfType<FurnitureItem>().Any());

        // The statistics by category: the furniture is grouped by the category (GroupBy), and for every group the number of items
        // and the total area of their footprints are calculated (rounded to 2 decimals). The categories are sorted from the most
        // numerous one to the least numerous one; categories with the same count go in alphabetical order (Ordinal comparison,
        // so the order does not depend on the regional settings).
        List<CategoryStatDto> categories = furniture
            .GroupBy(f => f.Category)
            .Select(g => new CategoryStatDto(g.Key, g.Count(), Math.Round(g.Sum(FootprintArea), 2)))
            .OrderByDescending(c => c.Count)
            .ThenBy(c => c.Category, StringComparer.Ordinal)
            .ToList();

        // The five largest items: the furniture is sorted by the area of the footprint, the largest first (items with the same
        // area go in the order of their ids, so the result is always the same); then the first five are taken (Take) and
        // converted to the DTO with the footprint area rounded to 2 decimals. If there are fewer than five items, all of them are taken.
        List<LargestItemDto> top5Largest = furniture
            .OrderByDescending(FootprintArea)
            .ThenBy(f => f.Id)
            .Take(5)
            .Select(f => new LargestItemDto(f.Id, f.Name, f.Category, Math.Round(FootprintArea(f), 2)))
            .ToList();

        return new PlanSummaryDto(
            plan.Id,
            plan.Name,
            rooms.Count,
            Math.Round(totalRoomArea, 2),
            furniture.Count,
            hasEmptyRooms,
            categories,
            top5Largest);
    }

    private static double FootprintArea(FurnitureItem item)
    {
        return item.Width * item.Depth;
    }
}
