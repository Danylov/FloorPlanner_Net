using FloorPlanner.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloorPlanner.Api.Data.Configurations;

/// <summary>EF Core mapping of <see cref="PlanObject"/> and its subclasses (table per hierarchy).</summary>
public sealed class PlanObjectConfiguration : IEntityTypeConfiguration<PlanObject>
{
    public void Configure(EntityTypeBuilder<PlanObject> builder)
    {
        builder.ToTable("plan_objects");

        builder.HasDiscriminator<string>("kind")
            .HasValue<FurnitureItem>("furniture")
            .HasValue<Door>("door")
            .HasValue<Window>("window");

        builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
    }
}
