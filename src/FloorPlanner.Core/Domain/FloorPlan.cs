namespace FloorPlanner.Core.Domain;

/// <summary>A floor plan: a named collection of rooms.</summary>
public sealed class FloorPlan : DomainObject
{
    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public List<Room> Rooms { get; } = new();

    public FloorPlan(string name)
    {
        Rename(name);
        CreatedAt = DateTime.UtcNow;
    }

    private FloorPlan()
    {
    }
    
    private void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}
