namespace FloorPlanner.Core.Geometry;

/// <summary>Polygon and segment helpers on the XZ floor plane.</summary>
public static class PolygonMath
{
    private const double BoundaryTolerance = 1e-4;

    /// <summary>
    /// Determines whether a point lies inside a polygon; points on the boundary (within the tolerance) count as inside.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order (clockwise or counter-clockwise); the last vertex connects back to the first.</param>
    /// <param name="point">The point to test.</param>
    /// <returns><see langword="true"/> if the point is inside the polygon or on its boundary; otherwise <see langword="false"/>.</returns>
    public static bool Contains(IReadOnlyList<Vector2> polygon, Vector2 point)
    {
        ArgumentNullException.ThrowIfNull(polygon);

        int count = polygon.Count;

        // The even-odd test below handles points lying exactly on the boundary unstably,
        // so such points are caught first: any point within the tolerance of an edge counts as inside.
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            if (DistanceToSegment(point, polygon[j], polygon[i]) <= BoundaryTolerance)
            {
                return true;
            }
        }

        // Even-odd rule (ray casting): count the edges that cross the horizontal line Y = point.Y
        // to the right of the point, toggling the flag on each one. An odd count means the point is inside.
        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];

            if ((vi.Y > point.Y) != (vj.Y > point.Y)
                && point.X < ((vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y)) + vi.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>Calculates the shortest distance from a point to a line segment.</summary>
    /// <param name="p">The point to measure from.</param>
    /// <param name="a">The first endpoint of the segment.</param>
    /// <param name="b">The second endpoint of the segment.</param>
    /// <returns>The distance in meters from <paramref name="p"/> to the closest point of the segment.</returns>
    private static double DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double lengthSquared = (dx * dx) + (dy * dy);

        double t = lengthSquared == 0
            ? 0
            : Math.Clamp((((p.X - a.X) * dx) + ((p.Y - a.Y) * dy)) / lengthSquared, 0, 1);

        double closestX = a.X + (t * dx);
        double closestY = a.Y + (t * dy);

        return Math.Sqrt(Math.Pow(p.X - closestX, 2) + Math.Pow(p.Y - closestY, 2));
    }
}
