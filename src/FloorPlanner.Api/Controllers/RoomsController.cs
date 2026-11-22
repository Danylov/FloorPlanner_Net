using FloorPlanner.Api.Dtos;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using FloorPlanner.Core.Geometry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Controllers;

/// <summary>CRUD operations for the rooms of a floor plan.</summary>
[ApiController]
[Route("api/floorplans/{planId:int}/rooms")]
public sealed class RoomsController(IRepository<FloorPlan> plans, IRepository<Room> rooms) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetAll(int planId, CancellationToken cancellationToken)
    {
        await EnsurePlanExistsAsync(planId, cancellationToken);

        List<Room> planRooms = await rooms.Query()
            .Where(r => r.FloorPlanId == planId)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);

        // The area is calculated in memory: RoomCalculator cannot be translated to SQL.
        return Ok(planRooms.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int planId, int id, CancellationToken cancellationToken)
    {
        Room room = await GetRoomAsync(planId, id, cancellationToken);

        return Ok(ToDto(room));
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(int planId, SaveRoomRequest request, CancellationToken cancellationToken)
    {
        FloorPlan plan = await plans.GetByIdAsync(planId, cancellationToken)
            ?? throw new NotFoundException("FloorPlan", planId);

        Room room = new(request.Name, ToCorners(request), request.Height);

        plan.Rooms.Add(room);

        await rooms.SaveChangesAsync(cancellationToken);

        RoomDto dto = ToDto(room);

        // Responds with 201 Created. The Location header holds the URL of the new room (built from GetById and the route values),
        // so the client can read it right away; the client is not required to store it, and the server remembers nothing.
        return CreatedAtAction(nameof(GetById), new { planId, id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int planId, int id, SaveRoomRequest request, CancellationToken cancellationToken)
    {
        Room room = await GetRoomAsync(planId, id, cancellationToken);

        room.Update(request.Name, ToCorners(request), request.Height);

        await rooms.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int planId, int id, CancellationToken cancellationToken)
    {
        Room room = await GetRoomAsync(planId, id, cancellationToken);

        rooms.Remove(room);

        // The objects of the room are deleted by the cascade rule of the foreign key.
        await rooms.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task EnsurePlanExistsAsync(int planId, CancellationToken cancellationToken)
    {
        if (await plans.GetByIdAsync(planId, cancellationToken) is null)
        {
            throw new NotFoundException("FloorPlan", planId);
        }
    }

    private async Task<Room> GetRoomAsync(int planId, int id, CancellationToken cancellationToken)
    {
        await EnsurePlanExistsAsync(planId, cancellationToken);

        return await rooms.Query().FirstOrDefaultAsync(r => r.Id == id && r.FloorPlanId == planId, cancellationToken)
            ?? throw new NotFoundException("Room", id);
    }

    private static IEnumerable<Vector2> ToCorners(SaveRoomRequest request)
    {
        return request.Corners.Select(p => new Vector2(p.X, p.Z));
    }

    private static RoomDto ToDto(Room room)
    {
        List<PointDto> corners = room.Corners.Select(c => new PointDto(c.X, c.Y)).ToList();

        return new RoomDto(room.Id, room.FloorPlanId, room.Name, room.Height, corners, RoomCalculator.Area(room));
    }
}
