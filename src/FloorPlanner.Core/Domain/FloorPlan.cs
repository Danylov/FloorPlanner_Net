using JetBrains.Annotations;

namespace FloorPlanner.Core.Domain;

/// <summary>A floor plan: a named collection of rooms.</summary>
public sealed class FloorPlan : DomainObject
{
    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public List<Room> Rooms { get; } = [];

    public FloorPlan(string name)
    {
        Rename(name);
        CreatedAt = DateTime.UtcNow;
    }

    [UsedImplicitly] // Called by EF Core through reflection.
    private FloorPlan()
    {
    }
    
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}
