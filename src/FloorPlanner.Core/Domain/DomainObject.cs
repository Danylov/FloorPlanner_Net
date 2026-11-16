namespace FloorPlanner.Core.Domain;

/// <summary>Base class for domain objects that have a database identity.</summary>
public abstract class DomainObject
{
    public int Id { get; protected set; }
}
