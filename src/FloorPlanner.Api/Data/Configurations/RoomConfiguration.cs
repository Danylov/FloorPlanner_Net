using System.Text.Json;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Geometry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloorPlanner.Api.Data.Configurations;

/// <summary>EF Core mapping of <see cref="Room"/>.</summary>
public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

        // The corners are stored as a single jsonb column; the comparer lets EF detect changes inside the list.
        builder.Property(r => r.Corners)
            .HasColumnType("jsonb")
            .HasConversion(
                corners => JsonSerializer.Serialize(corners, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<Vector2>>(json, (JsonSerializerOptions?)null) ?? new List<Vector2>(),
                new ValueComparer<List<Vector2>>(
                    (left, right) => left!.SequenceEqual(right!),
                    corners => corners.Aggregate(0, (hash, corner) => HashCode.Combine(hash, corner)),
                    corners => corners.ToList()));

        builder.HasMany(r => r.Objects)
            .WithOne(o => o.Room)
            .HasForeignKey(o => o.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
