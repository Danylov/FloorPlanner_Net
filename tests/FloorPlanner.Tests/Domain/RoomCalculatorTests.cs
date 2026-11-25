using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Tests.Domain;

/// <summary>Tests for <see cref="RoomCalculator"/>.</summary>
public sealed class RoomCalculatorTests
{
    private static readonly Vector2[] Rectangle = [new(0, 0), new(5, 0), new(5, 4), new(0, 4)];

    private static readonly Vector2[] LShape = [new(0, 0), new(4, 0), new(4, 2), new(2, 2), new(2, 4), new(0, 4)];

    private static readonly Vector2[] Triangle = [new(0, 0), new(4, 0), new(0, 3)];

    private static readonly Vector2[] ClockwiseRectangle = [new(0, 0), new(0, 4), new(5, 4), new(5, 0)];

    /// <summary>The area of a rectangle is width times depth.</summary>
    [Fact]
    public void Area_Rectangle_ReturnsProduct()
    {
        Assert.Equal(20, RoomCalculator.Area(Rectangle), precision: 4);
    }

    /// <summary>The perimeter of a rectangle is the sum of its four sides.</summary>
    [Fact]
    public void Perimeter_Rectangle_ReturnsSumOfSides()
    {
        Assert.Equal(18, RoomCalculator.Perimeter(Rectangle), precision: 4);
    }

    /// <summary>The volume is the area multiplied by the room height.</summary>
    [Fact]
    public void Volume_Rectangle_ReturnsAreaTimesHeight()
    {
        Room room = new("Hall", Rectangle);

        Assert.Equal(54, RoomCalculator.Volume(room), precision: 4);
    }

    /// <summary>The area of an L-shaped polygon is calculated correctly.</summary>
    [Fact]
    public void Area_LShape_ReturnsTwelve()
    {
        Assert.Equal(12, RoomCalculator.Area(LShape), precision: 4);
    }

    /// <summary>The perimeter of an L-shaped polygon is calculated correctly.</summary>
    [Fact]
    public void Perimeter_LShape_ReturnsSixteen()
    {
        Assert.Equal(16, RoomCalculator.Perimeter(LShape), precision: 4);
    }

    /// <summary>The room overload of the perimeter uses the room corners.</summary>
    [Fact]
    public void Perimeter_TrapezoidRoom_ReturnsTwenty()
    {
        Room room = new("Hall", [new(0, 0), new(7, 0), new(4, 4), new(0, 4)]);

        Assert.Equal(20, RoomCalculator.Perimeter(room), precision: 4);
    }

    /// <summary>The area of a right triangle is half the product of its legs.</summary>
    [Fact]
    public void Area_Triangle_ReturnsSix()
    {
        Assert.Equal(6, RoomCalculator.Area(Triangle), precision: 4);
    }

    /// <summary>The perimeter of a 3-4-5 triangle is twelve.</summary>
    [Fact]
    public void Perimeter_Triangle_ReturnsTwelve()
    {
        Assert.Equal(12, RoomCalculator.Perimeter(Triangle), precision: 4);
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

    // Commit 17 of the educational pair "test -> fix" added this test as a failing one: a clockwise polygon had
    // a negative area (-20 instead of 20). The bug is fixed in commit 18 (Area returns Math.Abs of the signed area).
    /// <summary>The area of a polygon does not depend on the direction of traversal: a clockwise polygon has a positive area.</summary>
    [Fact]
    public void Area_ClockwisePolygon_ReturnsPositive()
    {
        Assert.Equal(20, RoomCalculator.Area(ClockwiseRectangle), precision: 4);
    }

    // Commit 17 of the educational pair "test -> fix" added this test as a failing one (the volume was -54 instead of 54,
    // because of the negative area). It is green since the fix in commit 18.
    /// <summary>The volume does not depend on the direction of traversal: a clockwise polygon has a positive volume.</summary>
    [Fact]
    public void Volume_ClockwisePolygon_ReturnsPositive()
    {
        Room room = new("Hall", ClockwiseRectangle);

        Assert.Equal(54, RoomCalculator.Volume(room), precision: 4);
    }

    /// <summary>The signed area of a counter-clockwise polygon is positive.</summary>
    [Fact]
    public void SignedArea_CounterClockwisePolygon_ReturnsPositive()
    {
        Assert.Equal(20, RoomCalculator.SignedArea(Rectangle), precision: 4);
    }

    /// <summary>The signed area of a clockwise polygon is negative.</summary>
    [Fact]
    public void SignedArea_ClockwisePolygon_ReturnsNegative()
    {
        Assert.Equal(-20, RoomCalculator.SignedArea(ClockwiseRectangle), precision: 4);
    }

    /// <summary>A counter-clockwise polygon is not clockwise.</summary>
    [Fact]
    public void IsClockwise_CounterClockwisePolygon_ReturnsFalse()
    {
        Assert.False(RoomCalculator.IsClockwise(Rectangle));
    }

    /// <summary>A clockwise polygon is clockwise.</summary>
    [Fact]
    public void IsClockwise_ClockwisePolygon_ReturnsTrue()
    {
        Assert.True(RoomCalculator.IsClockwise(ClockwiseRectangle));
    }
}
