using FloorPlanner.Core.Placement;

namespace FloorPlanner.Core.Exceptions;

/// <summary>Thrown when an object cannot be placed in a room.</summary>
public sealed class PlacementException(PlacementResult result) : DomainException(GetMessage(result))
{
    public PlacementFailure Failure => result.Failure;

    public int? ConflictingObjectId => result.ConflictingObjectId;

    private static string GetMessage(PlacementResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Message ?? "The object cannot be placed in the room.";
    }
}
