namespace FloorPlanner.Core.Placement;

/// <summary>The outcome of checking whether an object can be placed in a room.</summary>
public sealed record PlacementResult(bool IsValid, PlacementFailure Failure, string? Message, int? ConflictingObjectId)
{
    public static PlacementResult Ok()
    {
        return new PlacementResult(true, PlacementFailure.None, null, null);
    }
}
