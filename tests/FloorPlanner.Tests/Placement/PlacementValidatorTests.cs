using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;
using FloorPlanner.Core.Placement;

namespace FloorPlanner.Tests.Placement;

/// <summary>Tests for <see cref="PlacementValidator"/>.</summary>
public sealed class PlacementValidatorTests
{
    private readonly PlacementValidator _validator = new();

    private static Room CreateRoom()
    {
        return new Room("Hall", [new Vector2(0, 0), new Vector2(5, 0), new Vector2(5, 4), new Vector2(0, 4)]);
    }

    private static FurnitureItem CreateItem(string name, double x, double z)
    {
        return new FurnitureItem(name, "Box", x, z, 1, 1, 1);
    }

    /// <summary>An item well inside the room is valid.</summary>
    [Fact]
    public void Validate_ItemInsideRoom_ReturnsOk()
    {
        PlacementResult result = _validator.Validate(CreateRoom(), CreateItem("A", 2.5, 2));

        Assert.True(result.IsValid);
        Assert.Equal(PlacementFailure.None, result.Failure);
    }

    /// <summary>An item whose side touches a wall is valid.</summary>
    [Fact]
    public void Validate_ItemTouchingWall_ReturnsOk()
    {
        PlacementResult result = _validator.Validate(CreateRoom(), CreateItem("A", 0.5, 2));

        Assert.True(result.IsValid);
    }

    /// <summary>An item sticking out through a wall is rejected.</summary>
    [Fact]
    public void Validate_ItemOutsideRoom_ReturnsOutsideRoom()
    {
        PlacementResult result = _validator.Validate(CreateRoom(), CreateItem("A", 0.2, 2));

        Assert.False(result.IsValid);
        Assert.Equal(PlacementFailure.OutsideRoom, result.Failure);
        Assert.Equal("Object 'A' is outside room 'Hall'.", result.Message);
    }

    /// <summary>An item overlapping another one is rejected.</summary>
    [Fact]
    public void Validate_OverlappingItems_ReturnsOverlapsObject()
    {
        Room room = CreateRoom();
        room.Objects.Add(CreateItem("A", 2, 2));

        PlacementResult result = _validator.Validate(room, CreateItem("B", 2.5, 2));

        Assert.False(result.IsValid);
        Assert.Equal(PlacementFailure.OverlapsObject, result.Failure);
        Assert.Equal("Object 'B' overlaps 'A'.", result.Message);
    }

    /// <summary>Items that only touch along a side do not conflict.</summary>
    [Fact]
    public void Validate_TouchingItems_ReturnsOk()
    {
        Room room = CreateRoom();
        room.Objects.Add(CreateItem("A", 1, 1));

        PlacementResult result = _validator.Validate(room, CreateItem("B", 2, 1));

        Assert.True(result.IsValid);
    }

    /// <summary>Doors are not checked.</summary>
    [Fact]
    public void Validate_Door_ReturnsOk()
    {
        Door door = new("Front", 2.5, 0, 0.9, 0.1, 2.1);

        PlacementResult result = _validator.Validate(CreateRoom(), door);

        Assert.True(result.IsValid);
    }

    /// <summary>An item already in the room does not conflict with itself.</summary>
    [Fact]
    public void Validate_CandidateAlreadyInRoom_DoesNotConflictWithItself()
    {
        Room room = CreateRoom();
        FurnitureItem item = CreateItem("A", 2.5, 2);
        room.Objects.Add(item);

        PlacementResult result = _validator.Validate(room, item);

        Assert.True(result.IsValid);
    }
}
