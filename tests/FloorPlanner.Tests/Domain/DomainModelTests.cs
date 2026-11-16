using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Tests.Domain;

/// <summary>Tests for the domain model classes.</summary>
public sealed class DomainModelTests
{
    /// <summary>A furniture item with zero width is rejected.</summary>
    [Fact]
    public void FurnitureItem_ZeroWidth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FurnitureItem("Oak", "Chair", 1, 1, 0, 0.45, 0.9));
    }

    /// <summary>A door with an empty name is rejected.</summary>
    [Fact]
    public void Door_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Door(string.Empty, 1, 0, 0.9, 0.1, 2.1));
    }

    /// <summary>A room with fewer than three corners is rejected.</summary>
    [Fact]
    public void Room_TwoCorners_Throws()
    {
        Vector2[] corners = [new(0, 0), new(4, 0)];

        Assert.Throws<ArgumentException>(() => new Room("Hall", corners));
    }

    /// <summary>Describe returns a type-specific text when called through the base class.</summary>
    [Fact]
    public void Describe_Polymorphic()
    {
        List<PlanObject> objects =
        [
            new FurnitureItem("Oak", "Chair", 1, 1, 0.45, 0.45, 0.9),
            new Door("Front", 2, 0, 0.9, 0.1, 2.1),
            new Window("W1", 3, 0, 1.2, 0.1, 1.2),
        ];

        Assert.Equal("Chair 'Oak' 0.45x0.45 m", objects[0].Describe());
        Assert.Equal("Door 'Front' 0.9 m wide", objects[1].Describe());
        Assert.Equal("Window 'W1' 1.2 m wide", objects[2].Describe());
    }

    /// <summary>Only furniture occupies floor space; doors and windows do not.</summary>
    [Fact]
    public void OccupiesFloor_ByType()
    {
        PlanObject furniture = new FurnitureItem("Oak", "Chair", 1, 1, 0.45, 0.45, 0.9);
        PlanObject door = new Door("Front", 2, 0, 0.9, 0.1, 2.1);
        PlanObject window = new Window("W1", 3, 0, 1.2, 0.1, 1.2);

        Assert.True(furniture.OccupiesFloor);
        Assert.False(door.OccupiesFloor);
        Assert.False(window.OccupiesFloor);
    }

    /// <summary>Update replaces all values of a furniture item.</summary>
    [Fact]
    public void Update_ChangesValues()
    {
        FurnitureItem item = new("Oak", "Chair", 1, 1, 0.45, 0.45, 0.9);

        item.Update("Pine", "Table", 2, 3, 1.5, 0.8, 0.75);

        Assert.Equal("Pine", item.Name);
        Assert.Equal("Table", item.Category);
        Assert.Equal(2, item.X, precision: 9);
        Assert.Equal(3, item.Z, precision: 9);
        Assert.Equal(1.5, item.Width, precision: 9);
        Assert.Equal(0.8, item.Depth, precision: 9);
        Assert.Equal(0.75, item.Height, precision: 9);
    }

    /// <summary>Update replaces the name, corners and height of a room.</summary>
    [Fact]
    public void Room_Update_ChangesValues()
    {
        Room room = new("Hall", [new(0, 0), new(4, 0), new(4, 3), new(0, 3)], 2.7);

        room.Update("Kitchen", [new(0, 0), new(5, 0), new(5, 4), new(0, 4)], 3.0);

        Assert.Equal("Kitchen", room.Name);
        Assert.Equal(3.0, room.Height, precision: 9);
        Assert.Equal(4, room.Corners.Count);
        Assert.Equal(new Vector2(5, 4), room.Corners[2]);
    }

    /// <summary>Update with fewer than three corners is rejected and leaves the room unchanged.</summary>
    [Fact]
    public void Room_Update_TwoCorners_ThrowsAndKeepsValues()
    {
        Room room = new("Hall", [new(0, 0), new(4, 0), new(4, 3), new(0, 3)], 2.7);

        Assert.Throws<ArgumentException>(() => room.Update("Kitchen", [new(0, 0), new(5, 0)], 3.0));

        Assert.Equal("Hall", room.Name);
        Assert.Equal(2.7, room.Height, precision: 9);
        Assert.Equal(4, room.Corners.Count);
    }

    /// <summary>The footprint is calculated from the center and the sizes.</summary>
    [Fact]
    public void Footprint_IsCalculatedFromCenterAndSize()
    {
        FurnitureItem item = new("Box", "Storage", 5, 5, 2, 1, 1);

        Rect footprint = item.Footprint;

        Assert.Equal(4, footprint.MinX, precision: 9);
        Assert.Equal(4.5, footprint.MinZ, precision: 9);
        Assert.Equal(6, footprint.MaxX, precision: 9);
        Assert.Equal(5.5, footprint.MaxZ, precision: 9);
    }
}
