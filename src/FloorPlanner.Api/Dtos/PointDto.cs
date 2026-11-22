namespace FloorPlanner.Api.Dtos;

/// <summary>A point on the floor plane in meters: X is world X, Z is world Z.</summary>
/// <param name="X">The coordinate along the X axis, in meters.</param>
/// <param name="Z">The coordinate along the Z axis, in meters (the <c>Y</c> field of the domain <c>Vector2</c>).</param>
// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record PointDto(double X, double Z);
// ReSharper restore NotAccessedPositionalProperty.Global
