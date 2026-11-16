using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Tests.Domain;

/// <summary>Tests for <see cref="RoomCalculator"/>.</summary>
public sealed class RoomCalculatorTests
{
    private static readonly Vector2[] Rectangle = [new(0, 0), new(5, 0), new(5, 4), new(0, 4)];

    private static readonly Vector2[] LShape = [new(0, 0), new(4, 0), new(4, 2), new(2, 2), new(2, 4), new(0, 4)];

    private static readonly Vector2[] Triangle = [new(0, 0), new(4, 0), new(0, 3)];

    /// <summary>The area of a rectangle is width times depth.</summary>
    [Fact]
    public void Area_Rectangle_ReturnsProduct()
    {
        Assert.Equal(20, RoomCalculator.Area(Rectangle), precision: 9);
    }

    /// <summary>The perimeter of a rectangle is the sum of its four sides.</summary>
    [Fact]
    public void Perimeter_Rectangle_ReturnsSumOfSides()
    {
        Assert.Equal(18, RoomCalculator.Perimeter(Rectangle), precision: 9);
    }

    /// <summary>The volume is the area multiplied by the room height.</summary>
    [Fact]
    public void Volume_Rectangle_ReturnsAreaTimesHeight()
    {
        Room room = new("Hall", Rectangle);

        Assert.Equal(54, RoomCalculator.Volume(room), precision: 9);
    }

    /// <summary>The area of an L-shaped polygon is calculated correctly.</summary>
    [Fact]
    public void Area_LShape_ReturnsTwelve()
    {
        Assert.Equal(12, RoomCalculator.Area(LShape), precision: 9);
    }

    /// <summary>The perimeter of an L-shaped polygon is calculated correctly.</summary>
    [Fact]
    public void Perimeter_LShape_ReturnsSixteen()
    {
        Assert.Equal(16, RoomCalculator.Perimeter(LShape), precision: 9);
    }

    /// <summary>The room overload of the perimeter uses the room corners.</summary>
    [Fact]
    public void Perimeter_TrapezoidRoom_ReturnsTwenty()
    {
        Room room = new("Hall", [new(0, 0), new(7, 0), new(4, 4), new(0, 4)]);

        Assert.Equal(20, RoomCalculator.Perimeter(room), precision: 9);
    }

    /// <summary>The area of a right triangle is half the product of its legs.</summary>
    [Fact]
    public void Area_Triangle_ReturnsSix()
    {
        Assert.Equal(6, RoomCalculator.Area(Triangle), precision: 9);
    }

    /// <summary>The perimeter of a 3-4-5 triangle is twelve.</summary>
    [Fact]
    public void Perimeter_Triangle_ReturnsTwelve()
    {
        Assert.Equal(12, RoomCalculator.Perimeter(Triangle), precision: 9);
    }

    /// <summary>Two points do not form a polygon, so the area is rejected.</summary>
    [Fact]
    public void Area_TwoPoints_Throws()
    {
        Vector2[] corners = [new(0, 0), new(4, 0)];

        Assert.Throws<ArgumentException>(() => RoomCalculator.Area(corners));
    }

    /// <summary>Two points do not form a polygon, so the perimeter is rejected.</summary>
    [Fact]
    public void Perimeter_TwoPoints_Throws()
    {
        Vector2[] corners = [new(0, 0), new(4, 0)];

        Assert.Throws<ArgumentException>(() => RoomCalculator.Perimeter(corners));
    }
}
