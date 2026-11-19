using FloorPlanner.Api.Dtos;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Controllers;

/// <summary>CRUD operations for floor plans.</summary>
// Why the actions return Task<ActionResult<T>> and not just T:
//
// - Task: the action is asynchronous. It reads the database with ToListAsync and similar methods,
//   so the thread is not blocked while the database responds and can serve other requests meanwhile.
//   A method returning a plain List<FloorPlanDto> would be synchronous and would block the thread.
//
// - ActionResult<T>: the action can return either the data or an HTTP response with a chosen status code
//   (Ok, CreatedAtAction, NotFound, ...). A plain T would always mean status 200.
//   Swagger also reads T from the type and shows it as the response body in the documentation.
//
// In short: Task<ActionResult<T>> is the standard signature of an asynchronous controller action
// that returns data and can control the response status code.
[ApiController]
[Route("api/floorplans")]
public sealed class FloorPlansController(IRepository<FloorPlan> plans) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FloorPlanDto>>> GetAll(CancellationToken cancellationToken)
    {
        // The room count is calculated in the database as part of the projection.
        List<FloorPlanDto> result = await plans.Query()
            .OrderBy(p => p.Id)
            .Select(p => new FloorPlanDto(p.Id, p.Name, p.CreatedAt, p.Rooms.Count))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FloorPlanDto>> GetById(int id, CancellationToken cancellationToken)
    {
        FloorPlan plan = await plans.GetByIdAsync(id, [p => p.Rooms], cancellationToken)
            ?? throw new NotFoundException("FloorPlan", id);

        return Ok(ToDto(plan));
    }

    [HttpPost]
    public async Task<ActionResult<FloorPlanDto>> Create(SaveFloorPlanRequest request, CancellationToken cancellationToken)
    {
        FloorPlan plan = new(request.Name);

        await plans.AddAsync(plan, cancellationToken);
        await plans.SaveChangesAsync(cancellationToken);

        FloorPlanDto dto = ToDto(plan);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveFloorPlanRequest request, CancellationToken cancellationToken)
    {
        FloorPlan plan = await plans.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("FloorPlan", id);

        plan.Rename(request.Name);

        await plans.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        FloorPlan plan = await plans.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("FloorPlan", id);

        plans.Remove(plan);

        // The rooms of the plan are deleted by the cascade rule of the foreign key.
        await plans.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static FloorPlanDto ToDto(FloorPlan plan)
    {
        return new FloorPlanDto(plan.Id, plan.Name, plan.CreatedAt, plan.Rooms.Count);
    }
}
