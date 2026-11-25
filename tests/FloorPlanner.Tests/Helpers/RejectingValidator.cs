using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Placement;

namespace FloorPlanner.Tests.Helpers;

// What this class is for (commit 19): it is used in the test CreateAsync_ValidatorRejects_Throws to check one specific thing:
// FurnitureService uses exactly the validator that was passed to its constructor and does not decide by itself.
//
// Why the other tests cannot show this: the test "an item outside the room" passes even if the service creates
// its own PlacementValidator inside, or repeats the geometric check itself, because the real validator would reject the item anyway.
//
// With RejectingValidator the picture is different:
// - the item is perfectly valid, so the real validator would accept it;
// - if the service asks the injected validator, it gets a rejection and throws PlacementException: the test passes;
// - if the service ignores the injected validator, the item is accepted, there is no exception, and the test fails.
//
// This works only because the service receives the validator through the constructor as an interface (DI): the test
// passes another object with the same interface and nothing has to be changed in the service.
// The class is not used in the application itself: the application works with the real PlacementValidator.

/// <summary>
/// A hand-written stub of the placement validator, used only in tests. It does not check anything and always answers "rejected",
/// so a test can make the service receive a rejection even for a valid item and check that the service obeys the injected validator.
/// </summary>
public sealed class RejectingValidator : IPlacementValidator
{
    public PlacementResult Validate(Room room, PlanObject candidate)
    {
        return new PlacementResult(false, PlacementFailure.OverlapsObject, "Rejected by the test validator.", null);
    }
}
