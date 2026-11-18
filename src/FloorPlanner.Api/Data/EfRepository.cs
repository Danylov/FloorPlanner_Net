using System.Linq.Expressions;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FloorPlanner.Api.Data;

/// <summary>
/// A generic EF Core repository (<see cref="IRepository{T}"/>) for working with the data of any domain entity configured in <see cref="AppDbContext"/>:
/// get by id, query, add, remove and save. It knows nothing about the domain rules.
/// </summary>
/// <typeparam name="T">The type of the domain object.</typeparam>
/// <remarks>
/// Every asynchronous method takes a <see cref="CancellationToken"/> named <c>ct</c>. The token is a signal that the work is no longer needed,
/// for example because the client closed the connection or the application is stopping. It is passed down to EF Core,
/// which then cancels the running database command instead of finishing a query nobody is waiting for, and throws <see cref="OperationCanceledException"/>.
/// The default value lets callers that do not need cancellation omit the argument.
/// </remarks>
public sealed class EfRepository<T>(AppDbContext db) : IRepository<T> // Constructor injection (DI): the container passes AppDbContext
    where T : DomainObject
{
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="ct">A token that cancels the database operation if the caller no longer needs the result.</param>
    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Set<T>().FindAsync([id], ct);
    }

    /// <param name="id">The identifier of the entity.</param>
    /// <param name="includes">The related data to load together with the entity (eager loading).</param>
    /// <param name="ct">A token that cancels the database operation if the caller no longer needs the result.</param>
    public async Task<T?> GetByIdAsync(int id, IEnumerable<Expression<Func<T, object?>>> includes, CancellationToken ct = default)
    {
        IQueryable<T> query = db.Set<T>();

        // Eager loading: every include is loaded together with the entity in a single query.
        foreach (Expression<Func<T, object?>> include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public IQueryable<T> Query()
    {
        return db.Set<T>();
    }

    /// <param name="entity">The entity to add.</param>
    /// <param name="ct">A token that cancels the database operation if the caller no longer needs the result.</param>
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await db.Set<T>().AddAsync(entity, ct);
    }

    public void Remove(T entity)
    {
        db.Set<T>().Remove(entity);
    }

    /// <param name="ct">A token that cancels the database operation if the caller no longer needs the result.</param>
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
