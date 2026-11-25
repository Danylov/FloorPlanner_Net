using System.Linq.Expressions;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;

namespace FloorPlanner.Tests.Helpers;

// What this class is for (commit 19): it replaces the real repository (EfRepository<T>, which works with PostgreSQL)
// in the tests of the services, so that the services can be checked without a database.
//
// How it works:
// - the entities are kept in the list Items, and nothing is written anywhere;
// - Seed and AddAsync give the entity an id (1, 2, ...) through reflection, the way the database would do it
//   (the Id property has a protected setter, so it cannot be assigned directly);
// - both overloads of GetByIdAsync look for the entity by id in the list; the includes are ignored, because the related objects
//   are already in memory (for example, the objects of a room are in room.Objects);
// - Remove takes the entity out of the list; SaveChangesAsync only counts the calls in SaveCalls, so a test can check
//   whether the service has saved the changes or not;
// - every asynchronous method starts with ct.ThrowIfCancellationRequested(), so a test with an already cancelled token
//   can check that the service stops before saving. The methods are not marked async (they return Task.FromResult
//   and Task.CompletedTask), otherwise the compiler would report the warning CS1998.
//
// Why a hand-written class: I decided to keep the set of NuGet packages in this project minimal. Only the packages
// that are really needed are connected, and mocking libraries such as Moq are not among them: the test doubles (like
// the mapping code) are written by hand. This also makes it explicit what exactly a test relies on.
// It is a "fake": unlike a stub with canned answers, it has a simple but working implementation
// (a list in memory and a counter of saves).

/// <summary>A hand-written test double of the repository: it keeps the entities in a list and counts the saves.</summary>
/// <typeparam name="T">The type of the domain object.</typeparam>
public sealed class FakeRepository<T> : IRepository<T>
    where T : DomainObject
{
    private int _nextId = 1;

    /// <summary>Gets the entities that are "stored" in the repository.</summary>
    private List<T> Items { get; } = [];

    /// <summary>Gets how many times <see cref="SaveChangesAsync"/> has been called.</summary>
    public int SaveCalls { get; private set; }

    /// <summary>Puts a ready entity into the repository, as if it had been stored earlier, and gives it an id (1, 2, ...).</summary>
    /// <param name="entity">The entity to put into the repository.</param>
    /// <returns>The same entity, with the id assigned.</returns>
    public T Seed(T entity)
    {
        AssignId(entity);
        Items.Add(entity);

        return entity;
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
    }

    // The includes are ignored: the entities are already in memory together with their related objects.
    public Task<T?> GetByIdAsync(int id, IEnumerable<Expression<Func<T, object?>>> includes, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
    }

    public IQueryable<T> Query()
    {
        return Items.AsQueryable();
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        Seed(entity);

        return Task.CompletedTask;
    }

    public void Remove(T entity)
    {
        Items.Remove(entity);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        SaveCalls++;

        return Task.CompletedTask;
    }

    // The id has a protected setter, so it is assigned through reflection (the way the database does it).
    private void AssignId(T entity)
    {
        typeof(DomainObject).GetProperty(nameof(DomainObject.Id))!.SetValue(entity, _nextId++);
    }
}
