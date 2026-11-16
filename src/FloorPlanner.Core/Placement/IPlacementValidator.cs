using FloorPlanner.Core.Domain;

namespace FloorPlanner.Core.Placement;

/// <summary>Checks whether an object can be placed in a room.</summary>
public interface IPlacementValidator
{
    PlacementResult Validate(Room room, PlanObject candidate);
}
