using System.Globalization;
using JetBrains.Annotations;

namespace FloorPlanner.Core.Domain;

/// <summary>A piece of furniture standing on the floor of a room.</summary>
public sealed class FurnitureItem : PlanObject
{
    private const int MaxCategoryLength = 50;

    public string Category { get; private set; } = string.Empty;

    public FurnitureItem(string name, string category, double x, double z, double width, double depth, double height)
        : base(name, x, z, width, depth, height)
    {
        ValidateCategory(category);
        Category = category;
    }

    [UsedImplicitly] // Called by EF Core through reflection.
    private FurnitureItem()
    {
    }

    public void Update(string name, string category, double x, double z, double width, double depth, double height)
    {
        ValidateCategory(category);
        Apply(name, x, z, width, depth, height);
        Category = category;
    }

    public override string Describe()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Category} '{Name}' {Width:0.##}x{Depth:0.##} m");
    }

    private static void ValidateCategory(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        if (category.Length > MaxCategoryLength)
        {
            throw new ArgumentException($"Category must not be longer than {MaxCategoryLength} characters.", nameof(category));
        }
    }
}
