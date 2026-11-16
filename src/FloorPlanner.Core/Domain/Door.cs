using System.Globalization;

namespace FloorPlanner.Core.Domain;

/// <summary>A door in a room wall; it does not occupy floor space.</summary>
public sealed class Door(string name, double x, double z, double width, double depth, double height)
    : PlanObject(name, x, z, width, depth, height)
{
    public override bool OccupiesFloor => false;

    public override string Describe()
    {
        return string.Create(CultureInfo.InvariantCulture, $"Door '{Name}' {Width:0.##} m wide");
    }
}
