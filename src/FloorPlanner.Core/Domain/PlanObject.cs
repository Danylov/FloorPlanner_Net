using FloorPlanner.Core.Geometry;

namespace FloorPlanner.Core.Domain;

/// <summary>Base class for anything placed in a room, defined by the center of its base and its sizes.</summary>
public abstract class PlanObject : DomainObject
{
    private const int MaxNameLength = 100;

    public string Name { get; private set; } = string.Empty;

    public double X { get; private set; }

    public double Z { get; private set; }

    public double Width { get; private set; }

    public double Depth { get; private set; }

    public double Height { get; private set; }

    public int RoomId { get; protected set; }

    public Rect Footprint => Rect.FromCenter(X, Z, Width, Depth);

    /// <summary>
    /// Gets a value indicating whether the object stands on the floor.
    /// <see langword="true"/>: the object stands on the floor (for example, furniture), so the placement validator checks it.
    /// <see langword="false"/>: the object does not stand on the floor (for example, a door or a window in a wall), so the placement validator does not check it.
    /// </summary>
    public virtual bool OccupiesFloor => true;

    protected PlanObject()
    {
    }

    protected PlanObject(string name, double x, double z, double width, double depth, double height)
    {
        Apply(name, x, z, width, depth, height);
    }

    public abstract string Describe();

    protected void Apply(string name, double x, double z, double width, double depth, double height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name must not be longer than {MaxNameLength} characters.", nameof(name));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Name = name;
        X = x;
        Z = z;
        Width = width;
        Depth = depth;
        Height = height;
    }
}
