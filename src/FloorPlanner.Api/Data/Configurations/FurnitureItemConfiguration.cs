using FloorPlanner.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloorPlanner.Api.Data.Configurations;

/// <summary>EF Core mapping of <see cref="FurnitureItem"/>.</summary>
public sealed class FurnitureItemConfiguration : IEntityTypeConfiguration<FurnitureItem>
{
    public void Configure(EntityTypeBuilder<FurnitureItem> builder)
    {
        builder.Property(f => f.Category).IsRequired().HasMaxLength(50);
    }
}
