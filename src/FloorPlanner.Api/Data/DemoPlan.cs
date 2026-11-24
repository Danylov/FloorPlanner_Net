using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Api.Data;

/// <summary>Builds the demo floor plan (without any database access, so that it can be tested).</summary>
public static class DemoPlan
{
    /// <summary>Creates the "Apartment" plan with two furnished rooms.</summary>
    /// <returns>A new plan that is not saved to the database yet.</returns>
    public static FloorPlan Build()
    {
        FloorPlan plan = new("Apartment");

        Room livingRoom = new("Living room", [new Vector2(0, 0), new Vector2(5, 0), new Vector2(5, 4), new Vector2(0, 4)]);
        livingRoom.Objects.Add(new FurnitureItem("Sofa", "Sofa", 1.5, 0.5, 2.0, 0.9, 0.9));
        livingRoom.Objects.Add(new FurnitureItem("Dining table", "Table", 3.5, 2.0, 1.2, 0.8, 0.75));
        livingRoom.Objects.Add(new FurnitureItem("Chair 1", "Chair", 3.5, 1.0, 0.45, 0.45, 0.9));
        livingRoom.Objects.Add(new FurnitureItem("Chair 2", "Chair", 3.5, 3.0, 0.45, 0.45, 0.9));
        livingRoom.Objects.Add(new Door("Entrance", 2.5, 4.0, 0.9, 0.1, 2.1));
        livingRoom.Objects.Add(new Window("Window 1", 1.5, 0.0, 1.2, 0.1, 1.2));

        Room bedroom = new("Bedroom", [new Vector2(0, 0), new Vector2(4, 0), new Vector2(4, 3.5), new Vector2(0, 3.5)]);
        bedroom.Objects.Add(new FurnitureItem("Bed", "Bed", 1.2, 1.2, 1.6, 2.0, 0.5));
        bedroom.Objects.Add(new FurnitureItem("Wardrobe", "Wardrobe", 3.3, 0.4, 1.0, 0.6, 2.1));
        bedroom.Objects.Add(new FurnitureItem("Nightstand", "Nightstand", 2.3, 0.4, 0.4, 0.4, 0.5));

        plan.Rooms.Add(livingRoom);
        plan.Rooms.Add(bedroom);

        return plan;
    }
}
