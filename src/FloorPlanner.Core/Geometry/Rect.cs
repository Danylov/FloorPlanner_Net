namespace FloorPlanner.Core.Geometry;

/// <summary>An axis-aligned rectangle on the XZ floor plane.</summary>
public readonly struct Rect
{
    public double MinX { get; }
    
    public double MaxX { get; }

    public double MinZ { get; }

    public double MaxZ { get; }

    /// <summary>Creates a rectangle from its minimum and maximum coordinates.</summary>
    /// <param name="minX">The smallest X coordinate (left edge).</param>
    /// <param name="minZ">The smallest Z coordinate (near edge).</param>
    /// <param name="maxX">The largest X coordinate (right edge).</param>
    /// <param name="maxZ">The largest Z coordinate (far edge).</param>
    /// <exception cref="ArgumentException"><paramref name="minX"/> is greater than <paramref name="maxX"/>, or <paramref name="minZ"/> is greater than <paramref name="maxZ"/>.</exception>
    public Rect(double minX, double minZ, double maxX, double maxZ)
    {
        if (minX > maxX)
        {
            throw new ArgumentException("MinX must not be greater than MaxX.", nameof(minX));
        }

        if (minZ > maxZ)
        {
            throw new ArgumentException("MinZ must not be greater than MaxZ.", nameof(minZ));
        }

        MinX = minX;
        MinZ = minZ;
        MaxX = maxX;
        MaxZ = maxZ;
    }

    /// <summary>Creates a rectangle from the center of its base and its sizes.</summary>
    /// <param name="centerX">The X coordinate of the center.</param>
    /// <param name="centerZ">The Z coordinate of the center.</param>
    /// <param name="width">The size along the X axis.</param>
    /// <param name="depth">The size along the Z axis.</param>
    /// <returns>The rectangle centered at (<paramref name="centerX"/>, <paramref name="centerZ"/>).</returns>
    public static Rect FromCenter(double centerX, double centerZ, double width, double depth)
    {
        double halfWidth = width / 2;
        double halfDepth = depth / 2;

        return new Rect(centerX - halfWidth, centerZ - halfDepth, centerX + halfWidth, centerZ + halfDepth);
    }

    /// <summary>Determines whether this rectangle overlaps another one; rectangles that only touch along a side do not intersect.</summary>
    /// <param name="other">The rectangle to test against.</param>
    /// <returns><see langword="true"/> if the interiors of the rectangles overlap; otherwise <see langword="false"/>.</returns>
    public bool Intersects(Rect other)
    {
        return MinX < other.MaxX && MaxX > other.MinX
            && MinZ < other.MaxZ && MaxZ > other.MinZ;
    }

    /// <summary>Gets the four corners of the rectangle.</summary>
    /// <returns>The corners in the order (MinX, MinZ), (MaxX, MinZ), (MaxX, MaxZ), (MinX, MaxZ).</returns>
    public IReadOnlyList<Vector2> Corners()
    {
        return
        [
            new Vector2(MinX, MinZ),
            new Vector2(MaxX, MinZ),
            new Vector2(MaxX, MaxZ),
            new Vector2(MinX, MaxZ),
        ];
    }
}
