namespace FloorPlanner.Core.Placement;

/// <summary>The reason why an object cannot be placed in a room.</summary>
public enum PlacementFailure
{
    None,
    OutsideRoom,
    OverlapsObject,
}
