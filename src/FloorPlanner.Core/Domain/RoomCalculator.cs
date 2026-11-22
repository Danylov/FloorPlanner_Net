using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Core.Domain;

/// <summary>Calculates the area, perimeter and volume of rooms.</summary>
public static class RoomCalculator
{
    /// <summary>Calculates the area of a polygon using the Gauss (shoelace) formula.</summary>
    public static double Area(IReadOnlyList<Vector2> corners)
    {
        ValidateCorners(corners);

        double sum = 0;

        for (int i = 0; i < corners.Count; i++)
        {
            Vector2 current = corners[i];
            Vector2 next = corners[(i + 1) % corners.Count];

            sum += (current.X * next.Y) - (next.X * current.Y);
        }

        return 0.5 * sum;
    }

    public static double Perimeter(IReadOnlyList<Vector2> corners)
    {
        ValidateCorners(corners);

        double sum = 0;

        for (int i = 0; i < corners.Count; i++)
        {
            Vector2 current = corners[i];
            Vector2 next = corners[(i + 1) % corners.Count];

            double dx = next.X - current.X;
            double dy = next.Y - current.Y;

            sum += Math.Sqrt((dx * dx) + (dy * dy));
        }

        return sum;
    }

    /// <summary>Calculates the floor area of a room using the Gauss (shoelace) formula.</summary>
    public static double Area(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return Area(room.Corners);
    }

    public static double Perimeter(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return Perimeter(room.Corners);
    }

    public static double Volume(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return Area(room) * room.Height;
    }

    private static void ValidateCorners(IReadOnlyList<Vector2> corners)
    {
        ArgumentNullException.ThrowIfNull(corners);

        if (corners.Count < 3)
        {
            throw new ArgumentException("A polygon must have at least 3 corners.", nameof(corners));
        }
    }
}
