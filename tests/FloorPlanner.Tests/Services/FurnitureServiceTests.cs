using FloorPlanner.Api.Services;
using FloorPlanner.Core.Domain;
using FloorPlanner.Core.Exceptions;
using FloorPlanner.Core.Geometry;
using FloorPlanner.Core.Placement;
using FloorPlanner.Tests.Helpers;

namespace FloorPlanner.Tests.Services;

// What commit 19 does: it checks the business logic of FurnitureService in isolation, quickly and without a database.
//
// The real repositories (EF Core and PostgreSQL) are replaced by hand-written test doubles: FakeRepository<T> keeps
// the entities in a list and counts the saves, and RejectingValidator always answers "rejected". The project does not use
// mocking libraries such as Moq (see FakeRepository for the reason), so the doubles are written by hand.
//
// The seven tests check the scenario of the service:
// - a valid item is added to the room and the changes are saved once;
// - an item outside the room, or an item that overlaps another one, is rejected: nothing is added and nothing is saved;
// - an unknown room gives NotFoundException;
// - an update into another item is rejected, the item stays at the old position and nothing is saved;
// - the service obeys the injected validator (RejectingValidator);
// - an already cancelled token stops the operation before anything is saved.
//
// Why this is needed in the project:
// - Speed and independence: the tests run in milliseconds, need neither Docker nor PostgreSQL, and do not depend on the data in a database.
// - DI in practice: the service gets the repositories and the validator through its constructor as interfaces, so a test
//   can simply pass other objects with the same interfaces (the "DI and IoC" skill, commits 8, 12 and 19).
// - CancellationToken: the cancellation test shows that the token really reaches the repository and stops the operation
//   before the changes are saved (the "async/await, CancellationToken" skill).
// - Protection against breaking: the placement rules and the rollback of a rejected update are now guarded automatically.
//
// These tests do not replace the manual check of the endpoints on a real database: they supplement it on the level of the service logic.

/// <summary>Tests for <see cref="FurnitureService"/> with a fake repository: no database is needed.</summary>
public sealed class FurnitureServiceTests
{
    private readonly FakeRepository<Room> _rooms = new();
    private readonly FakeRepository<PlanObject> _objects = new();
    private readonly Room _room;

    public FurnitureServiceTests()
    {
        // The room is 5 x 4 m: (0,0), (5,0), (5,4), (0,4).
        _room = _rooms.Seed(new Room("Hall", [new Vector2(0, 0), new Vector2(5, 0), new Vector2(5, 4), new Vector2(0, 4)]));
    }

    private static FurnitureSpec Spec(double x, double z)
    {
        return new FurnitureSpec("Box", "Storage", x, z, 1, 1, 1);
    }

    private FurnitureService CreateService(IPlacementValidator? validator = null)
    {
        return new FurnitureService(_rooms, _objects, validator ?? new PlacementValidator());
    }

    // Puts a 1 x 1 x 1 item into the room as if it had been created earlier (it gets an id).
    private FurnitureItem AddExisting(double x, double z)
    {
        FurnitureItem item = new("Box", "Storage", x, z, 1, 1, 1);

        _objects.Seed(item);
        _room.Objects.Add(item);

        return item;
    }

    /// <summary>A valid item is added to the room and the changes are saved once.</summary>
    [Fact]
    public async Task CreateAsync_ValidItem_AddsToRoomAndSaves()
    {
        FurnitureService service = CreateService();

        await service.CreateAsync(_room.Id, Spec(2.5, 2), CancellationToken.None);

        Assert.Single(_room.Objects);
        Assert.Equal(1, _rooms.SaveCalls);
    }

    /// <summary>An item outside the room is rejected, nothing is added and nothing is saved.</summary>
    [Fact]
    public async Task CreateAsync_OutsideRoom_ThrowsAndDoesNotSave()
    {
        FurnitureService service = CreateService();

        PlacementException exception = await Assert.ThrowsAsync<PlacementException>(
            () => service.CreateAsync(_room.Id, Spec(0.2, 2), CancellationToken.None));

        Assert.Equal(PlacementFailure.OutsideRoom, exception.Failure);
        Assert.Empty(_room.Objects);
        Assert.Equal(0, _rooms.SaveCalls);
    }

    /// <summary>An item that overlaps another one is rejected.</summary>
    [Fact]
    public async Task CreateAsync_Overlap_Throws()
    {
        AddExisting(2, 2);
        FurnitureService service = CreateService();

        PlacementException exception = await Assert.ThrowsAsync<PlacementException>(
            () => service.CreateAsync(_room.Id, Spec(2.5, 2), CancellationToken.None));

        Assert.Equal(PlacementFailure.OverlapsObject, exception.Failure);
    }

    /// <summary>A room that does not exist gives NotFoundException.</summary>
    [Fact]
    public async Task CreateAsync_UnknownRoom_ThrowsNotFound()
    {
        FurnitureService service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(999, Spec(2.5, 2), CancellationToken.None));
    }

    /// <summary>Moving an item onto another one is rejected, the item stays at the old position and nothing is saved.</summary>
    [Fact]
    public async Task UpdateAsync_IntoOtherItem_ThrowsAndKeepsOldPosition()
    {
        FurnitureItem first = AddExisting(1, 1);
        AddExisting(3, 3);
        FurnitureService service = CreateService();

        await Assert.ThrowsAsync<PlacementException>(
            () => service.UpdateAsync(_room.Id, first.Id, Spec(3.5, 3), CancellationToken.None));

        Assert.Equal(1, first.X, precision: 4);
        Assert.Equal(1, first.Z, precision: 4);
        Assert.Equal(0, _rooms.SaveCalls);
    }

    /// <summary>The service obeys the validator: if it rejects, even a valid item is not created (this is the benefit of DI).</summary>
    [Fact]
    public async Task CreateAsync_ValidatorRejects_Throws()
    {
        FurnitureService service = CreateService(new RejectingValidator());

        await Assert.ThrowsAsync<PlacementException>(
            () => service.CreateAsync(_room.Id, Spec(2.5, 2), CancellationToken.None));
    }

    /// <summary>An already cancelled token stops the operation before anything is saved.</summary>
    [Fact]
    public async Task CreateAsync_CancelledToken_ThrowsAndDoesNotSave()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        FurnitureService service = CreateService();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.CreateAsync(_room.Id, Spec(2.5, 2), cts.Token));

        Assert.Equal(0, _rooms.SaveCalls);
    }
}
