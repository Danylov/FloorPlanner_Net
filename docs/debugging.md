# Debugging: negative area of a clockwise polygon

> This is an **educational** debugging case (commits 17 and 18 of the project): the bug was planted on purpose
> (`Math.Abs` was left out of `RoomCalculator.Area` in commit 4), it is not a real production incident.
> The numbers in "Symptom" and "Reproduction" were taken from the running API before the fix; the numbers in
> "How it was found" were seen by me in a debugging session in Rider.

## Symptom

In the summary of a floor plan (`GET /api/floorplans/{id}/summary`) the field `totalRoomArea` is negative: **-20**
for a plan with a single 5 x 4 m room. The room itself (`GET /api/floorplans/{planId}/rooms`) has `"area": -20`,
and the volume of such a room would be -54 (area times the height 2.7).

## Reproduction

1. Create a floor plan: `POST /api/floorplans` with `{"name":"ClockwiseRepro"}`.
2. Add a room whose corners are listed **clockwise**: `POST /api/floorplans/{planId}/rooms` with

   ```json
   {"name":"Clockwise","height":2.7,"corners":[{"x":0,"z":0},{"x":0,"z":4},{"x":5,"z":4},{"x":5,"z":0}]}
   ```

   The answer contains `"area":-20`.
3. `GET /api/floorplans/{planId}/summary` returns `"roomCount":1` and `"totalRoomArea":-20`.

The same room with the corners listed counter-clockwise ((0,0), (5,0), (5,4), (0,4)) has the area 20.

A failing unit test was written first (commit 17): `Area_ClockwisePolygon_ReturnsPositive` expected 20 and got -20;
`Volume_ClockwisePolygon_ReturnsPositive` expected 54 and got -54.

## Hypotheses

1. **A mistake in the formula** (a wrong index or a wrong coefficient in the Gauss formula).
   Check: the tests of commit 4 are green (a rectangle gives 20, an L-shaped polygon 12, a triangle 6), so the formula
   gives correct numbers at least for the counter-clockwise traversal. The hypothesis is rejected.
2. **The order of the corners** (the direction of traversal). Check: the same rectangle gives 20 when the corners are
   listed counter-clockwise and -20 when they are listed clockwise, so the sign depends only on the direction.
   The hypothesis is confirmed.

## How it was found

The failing test `Area_ClockwisePolygon_ReturnsPositive` (commit 17) showed the symptom (expected 20, got -20),
and the debugger in Rider showed where the sign comes from. The test was run under the debugger with two breakpoints:

1. In `RoomCalculator.SignedArea` (the method that holds the code of the old `Area`), on the line `return 0.5 * sum;`.
   For the clockwise corners (0,0), (0,4), (5,4), (5,0) the Watch window shows `sum` = **-40**
   (the terms of the sum are 0, -20, -20, 0), and the returned value is 0.5 * (-40) = **-20**.
   So the formula itself works and gives the signed result: the sign is negative because of the direction of traversal.
2. In `RoomCalculator.Area`, on the line `return Math.Abs(SignedArea(corners));`.
   `SignedArea(corners)` is **-20** here, and `Area` returns **+20**: `Math.Abs` removes the sign.

After the fix both tests of commit 17 (`Area_ClockwisePolygon_ReturnsPositive` and `Volume_ClockwisePolygon_ReturnsPositive`)
are green, and so are all the other tests.

## Cause

The Gauss (shoelace) formula returns the **signed** area: positive when the corners are listed counter-clockwise and
negative when they are listed clockwise. Since commit 4 `RoomCalculator.Area` returned this value as it was
(`Math.Abs` was not applied on purpose), so any room whose corners were listed clockwise got a negative area,
and the negative number went further into the volume and into the plan summary.

## Fix

In `RoomCalculator` (commit 18):

- `SignedArea(corners)` returns the signed result of the Gauss formula (it contains the old code of `Area`);
- `IsClockwise(corners)` returns `SignedArea(corners) < 0`;
- `Area(corners)` now returns `Math.Abs(SignedArea(corners))`, so the area does not depend on the direction of traversal.

The sign is kept in a separate method on purpose: the direction of traversal may be needed later, and `Area` always
has to be non-negative.

Result on the running API after the fix: the same room has `"area": 20`, and its plan has `"totalRoomArea": 20`;
the demo plan "Apartment" is not affected, because the corners of its rooms are listed counter-clockwise.

## Regression tests

- `Area_ClockwisePolygon_ReturnsPositive` (commit 17): the clockwise rectangle gives 20.
- `Volume_ClockwisePolygon_ReturnsPositive` (commit 17): the same rectangle with the height 2.7 gives the volume 54.
- `SignedArea_CounterClockwisePolygon_ReturnsPositive`: 20 for (0,0), (5,0), (5,4), (0,4).
- `SignedArea_ClockwisePolygon_ReturnsNegative`: -20 for the clockwise traversal.
- `IsClockwise_CounterClockwisePolygon_ReturnsFalse` and `IsClockwise_ClockwisePolygon_ReturnsTrue`.
- All the tests of commit 4 (counter-clockwise polygons) stay green.

## Conclusion

- A failing test that reproduces the bug (commit 17) is written before the fix (commit 18): it proves that the bug
  exists and later proves that it is fixed.
- A formula that "works" on the examples can still depend on a hidden assumption (here: the direction of traversal);
  tests should cover the other direction too.
- The quantity that a user sees (the area) and the quantity that the formula gives (the signed area) are different things;
  it is better to keep them in different methods.
