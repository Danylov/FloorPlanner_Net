using System.Globalization;

namespace FloorPlanner.Core.Domain;

/// <summary>A window in a room wall; it does not occupy floor space.</summary>
public sealed class Window(string name, double x, double z, double width, double depth, double height)
    : PlanObject(name, x, z, width, depth, height)
{
    public override bool OccupiesFloor => false;

    public override string Describe()
    {
        return string.Create(CultureInfo.InvariantCulture, $"Window '{Name}' {Width:0.##} m wide");
    }
}
