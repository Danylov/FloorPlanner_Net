using FloorPlanner.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloorPlanner.Api.Data.Configurations;

/// <summary>EF Core mapping of <see cref="FloorPlan"/>.</summary>
public sealed class FloorPlanConfiguration : IEntityTypeConfiguration<FloorPlan>
{
    public void Configure(EntityTypeBuilder<FloorPlan> builder)
    {
        builder.ToTable("floor_plans");

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasMany(p => p.Rooms)
            .WithOne(r => r.FloorPlan)
            .HasForeignKey(r => r.FloorPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
