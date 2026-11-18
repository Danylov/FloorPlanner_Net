using FloorPlanner.Core.Domain;

namespace FloorPlanner.Core.Placement;

/// <summary>Checks whether an object can be placed in a room.</summary>
public interface IPlacementValidator // Abstraction: consumers depend on the interface, not on the implementation
{
    PlacementResult Validate(Room room, PlanObject candidate);
}
