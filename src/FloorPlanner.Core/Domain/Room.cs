using FloorPlanner.Core.Geometry;
using JetBrains.Annotations;

namespace FloorPlanner.Core.Domain;

/// <summary>A room described by a floor polygon and a height, containing plan objects.</summary>
public sealed class Room : DomainObject
{
    private const int MaxNameLength = 100;

    [UsedImplicitly] // The setter is called by EF Core when it fills the foreign key.
    public int FloorPlanId { get; private set; }

    [UsedImplicitly] // The setter is called by EF Core when it loads the related plan.
    public FloorPlan? FloorPlan { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public double Height { get; private set; }

    public List<Vector2> Corners { get; private set; } = [];

    public List<PlanObject> Objects { get; } = [];

    public Room(string name, IEnumerable<Vector2> corners, double height = 2.7)
    {
        Update(name, corners, height);
    }

    [UsedImplicitly] // Called by EF Core through reflection.
    private Room()
    {
    }

    public void Update(string name, IEnumerable<Vector2> corners, double height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name must not be longer than {MaxNameLength} characters.", nameof(name));
        }

        List<Vector2> cornerList = new(corners);

        if (cornerList.Count < 3)
        {
            throw new ArgumentException("A room must have at least 3 corners.", nameof(corners));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Name = name;
        Corners = cornerList;
        Height = height;
    }
}
