using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Api.Data;

/// <summary>Builds the demo floor plan (without any database access, so that it can be tested).</summary>
public static class DemoPlan
{
    /// <summary>Creates the "Apartment" plan with two furnished rooms (19 pieces of furniture, a door and a window).</summary>
    /// <returns>A new plan that is not saved to the database yet.</returns>
    public static FloorPlan Build()
    {
        FloorPlan plan = new("Apartment");

        Room livingRoom = new("Living room", [new Vector2(0, 0), new Vector2(7, 0), new Vector2(7, 5), new Vector2(0, 5)]);
        livingRoom.Objects.Add(new FurnitureItem("Sofa", "Sofa", 1.5, 0.5, 2.0, 0.9, 0.9));
        livingRoom.Objects.Add(new FurnitureItem("Dining table", "Table", 3.5, 2.0, 1.2, 0.8, 0.75));
        livingRoom.Objects.Add(new FurnitureItem("Chair 1", "Chair", 3.5, 1.0, 0.45, 0.45, 0.9));
        livingRoom.Objects.Add(new FurnitureItem("Chair 2", "Chair", 3.5, 3.0, 0.45, 0.45, 0.9));
        livingRoom.Objects.Add(new FurnitureItem("Bookshelf", "Shelf", 0.3, 2.0, 0.4, 1.2, 1.8));
        livingRoom.Objects.Add(new FurnitureItem("TV stand", "Cabinet", 0.3, 3.3, 0.4, 1.0, 0.5));
        livingRoom.Objects.Add(new FurnitureItem("Coffee table", "Table", 1.5, 2.0, 1.1, 0.6, 0.4));
        livingRoom.Objects.Add(new FurnitureItem("Side table", "Table", 2.75, 0.35, 0.5, 0.5, 0.5));
        livingRoom.Objects.Add(new FurnitureItem("Floor lamp", "Lamp", 4.7, 0.3, 0.3, 0.3, 1.6));
        livingRoom.Objects.Add(new FurnitureItem("Fern", "Plant", 4.6, 1.2, 0.4, 0.4, 0.8));
        livingRoom.Objects.Add(new Door("Entrance", 2.5, 5.0, 0.9, 0.1, 2.1));
        livingRoom.Objects.Add(new Window("Window 1", 1.5, 0.0, 1.2, 0.1, 1.2));

        Room bedroom = new("Bedroom", [new Vector2(0, 0), new Vector2(5, 0), new Vector2(5, 4), new Vector2(0, 4)]);
        bedroom.Objects.Add(new FurnitureItem("Bed", "Bed", 1.2, 1.2, 1.6, 2.0, 0.5));
        bedroom.Objects.Add(new FurnitureItem("Wardrobe", "Wardrobe", 3.3, 0.4, 1.0, 0.6, 2.1));
        bedroom.Objects.Add(new FurnitureItem("Nightstand", "Nightstand", 2.3, 0.4, 0.4, 0.4, 0.5));
        bedroom.Objects.Add(new FurnitureItem("Nightstand 2", "Nightstand", 0.2, 0.4, 0.35, 0.4, 0.5));
        bedroom.Objects.Add(new FurnitureItem("Dresser", "Dresser", 3.3, 2.0, 1.2, 0.45, 0.9));
        bedroom.Objects.Add(new FurnitureItem("Desk", "Desk", 3.2, 3.1, 1.2, 0.6, 0.75));
        bedroom.Objects.Add(new FurnitureItem("Office chair", "Chair", 3.2, 2.52, 0.5, 0.5, 1.0));
        bedroom.Objects.Add(new FurnitureItem("Bench", "Bench", 1.2, 2.55, 1.2, 0.4, 0.45));
        bedroom.Objects.Add(new FurnitureItem("Rubber plant", "Plant", 0.3, 3.2, 0.4, 0.4, 1.0));

        plan.Rooms.Add(livingRoom);
        plan.Rooms.Add(bedroom);

        return plan;
    }
}
