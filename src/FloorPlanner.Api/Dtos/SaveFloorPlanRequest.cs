using System.ComponentModel.DataAnnotations;

namespace FloorPlanner.Api.Dtos;

/// <summary>
/// The request body for creating or renaming a floor plan (a data transfer object).
/// It accepts only the name, so the client cannot set the id or the creation date,
/// and it carries the input validation rules (the name is required, 1 to 100 characters).
/// </summary>
public sealed record SaveFloorPlanRequest([Required, StringLength(100, MinimumLength = 1)] string Name);
