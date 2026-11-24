using FloorPlanner.Api.Data;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Placement;

namespace FloorPlanner.Tests.Data;

/// <summary>Tests for <see cref="DemoPlan"/>.</summary>
public sealed class DemoPlanTests
{
    /// <summary>Every piece of furniture of the demo plan is inside its room and does not overlap other furniture.</summary>
    [Fact]
    public void Build_AllFurniture_PassesPlacementRules()
    {
        FloorPlan plan = DemoPlan.Build();
        PlacementValidator validator = new();

        foreach (Room room in plan.Rooms)
        {
            foreach (PlanObject item in room.Objects.Where(o => o.OccupiesFloor))
            {
                PlacementResult result = validator.Validate(room, item);

                Assert.True(result.IsValid, $"{room.Name}: {result.Message}");
            }
        }
    }
}
