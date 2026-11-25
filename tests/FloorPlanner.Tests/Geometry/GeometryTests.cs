using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Tests.Geometry;

/// <summary>Tests for <see cref="Rect"/> and <see cref="PolygonMath"/>.</summary>
public sealed class GeometryTests
{
    /// <summary>A convex 4x3 rectangle with a corner at the origin.</summary>
    private static readonly Vector2[] Box =
    [
        new(0, 0), new(4, 0), new(4, 3), new(0, 3),
    ];

    /// <summary>A concave L-shaped polygon; the region x &gt; 2, y &gt; 2 is cut out.</summary>
    private static readonly Vector2[] LShape =
    [
        new(0, 0), new(4, 0), new(4, 2), new(2, 2), new(2, 4), new(0, 4),
    ];

    /// <summary>Width must map to the X axis and depth to the Z axis.</summary>
    [Fact]
    public void FromCenter_WidthAlongXDepthAlongZ_ReturnsExpectedBounds()
    {
        Rect rect = Rect.FromCenter(5, 5, 2, 1);

        Assert.Equal(4, rect.MinX, precision: 4);
        Assert.Equal(4.5, rect.MinZ, precision: 4);
        Assert.Equal(6, rect.MaxX, precision: 4);
        Assert.Equal(5.5, rect.MaxZ, precision: 4);
    }

    /// <summary>Corners must be returned in the order (MinX,MinZ), (MaxX,MinZ), (MaxX,MaxZ), (MinX,MaxZ).</summary>
    [Fact]
    public void Corners_ValidRect_ReturnsFourPointsInOrder()
    {
        IReadOnlyList<Vector2> corners = Rect.FromCenter(5, 5, 2, 1).Corners();

        Assert.Equal(4, corners.Count);
        Assert.Equal(new Vector2(4, 4.5), corners[0]);
        Assert.Equal(new Vector2(6, 4.5), corners[1]);
        Assert.Equal(new Vector2(6, 5.5), corners[2]);
        Assert.Equal(new Vector2(4, 5.5), corners[3]);
    }

    /// <summary>Rectangles whose interiors overlap intersect.</summary>
    [Fact]
    public void Intersects_Overlapping_ReturnsTrue()
    {
        Rect a = Rect.FromCenter(0, 0, 2, 2);
        Rect b = Rect.FromCenter(1, 0, 2, 2);

        Assert.True(a.Intersects(b));
    }

    /// <summary>Rectangles that only share a side do not intersect (the comparison is strict).</summary>
    [Fact]
    public void Intersects_TouchingSides_ReturnsFalse()
    {
        Rect a = Rect.FromCenter(0, 0, 2, 2);
        Rect b = Rect.FromCenter(2, 0, 2, 2);

        Assert.False(a.Intersects(b));
    }

    /// <summary>Rectangles far apart do not intersect.</summary>
    [Fact]
    public void Intersects_Far_ReturnsFalse()
    {
        Rect a = Rect.FromCenter(0, 0, 2, 2);
        Rect b = Rect.FromCenter(10, 10, 2, 2);

        Assert.False(a.Intersects(b));
    }

    /// <summary>The constructor rejects a rectangle whose MinX is greater than MaxX.</summary>
    [Fact]
    public void Constructor_MinXGreaterThanMaxX_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Rect(2, 0, 1, 1));
    }

    /// <summary>A point in the middle of the rectangle is inside.</summary>
    [Fact]
    public void Contains_PointInsideRectangle_ReturnsTrue()
    {
        Assert.True(PolygonMath.Contains(Box, new Vector2(2, 1)));
    }

    /// <summary>A point to the right of the rectangle is outside.</summary>
    [Fact]
    public void Contains_PointOutsideRectangle_ReturnsFalse()
    {
        Assert.False(PolygonMath.Contains(Box, new Vector2(5, 1)));
    }

    /// <summary>A point on an edge counts as inside.</summary>
    [Fact]
    public void Contains_PointOnBoundary_ReturnsTrue()
    {
        Assert.True(PolygonMath.Contains(Box, new Vector2(0, 2)));
    }

    /// <summary>A point in the cut-out part of the L-shape is outside.</summary>
    [Fact]
    public void Contains_PointInConcavePocket_ReturnsFalse()
    {
        Assert.False(PolygonMath.Contains(LShape, new Vector2(3, 3)));
    }

    /// <summary>A point in the solid part of the L-shape is inside.</summary>
    [Fact]
    public void Contains_PointInsideLShape_ReturnsTrue()
    {
        Assert.True(PolygonMath.Contains(LShape, new Vector2(1, 3)));
    }
}
