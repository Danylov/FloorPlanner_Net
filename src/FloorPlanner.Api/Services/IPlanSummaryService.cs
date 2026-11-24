using FloorPlanner.Api.Dtos;

namespace FloorPlanner.Api.Services;

/// <summary>Builds the summary of a floor plan.</summary>
public interface IPlanSummaryService
{
    Task<PlanSummaryDto> GetAsync(int planId, CancellationToken ct = default);
}
