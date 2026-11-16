using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Core.Placement;

/// <summary>Checks that an object lies inside a room and does not overlap other objects on the floor.</summary>
public sealed class PlacementValidator : IPlacementValidator
{
    public PlacementResult Validate(Room room, PlanObject candidate)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!candidate.OccupiesFloor)
        {
            return PlacementResult.Ok();
        }

        Rect footprint = candidate.Footprint;

        foreach (Vector2 corner in footprint.Corners())
        {
            if (!PolygonMath.Contains(room.Corners, corner))
            {
                return new PlacementResult(
                    false,
                    PlacementFailure.OutsideRoom,
                    $"Object '{candidate.Name}' is outside room '{room.Name}'.",
                    null);
            }
        }

        foreach (PlanObject other in room.Objects)
        {
            if (other.OccupiesFloor
                && !ReferenceEquals(other, candidate)
                && footprint.Intersects(other.Footprint))
            {
                return new PlacementResult(
                    false,
                    PlacementFailure.OverlapsObject,
                    $"Object '{candidate.Name}' overlaps '{other.Name}'.",
                    other.Id);
            }
        }

        return PlacementResult.Ok();
    }
}
