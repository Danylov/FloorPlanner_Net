using System.Linq.Expressions;
using FloorPlanner.Core.Domain;

namespace FloorPlanner.Core.Abstractions;

/// <summary>A generic repository that stores and loads domain objects.</summary>
/// <typeparam name="T">The type of the domain object.</typeparam>
public interface IRepository<T> // Abstraction: consumers depend on the interface, not on EF Core
    where T : DomainObject
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<T?> GetByIdAsync(int id, IEnumerable<Expression<Func<T, object?>>> includes, CancellationToken ct = default);

    IQueryable<T> Query();

    Task AddAsync(T entity, CancellationToken ct = default);

    void Remove(T entity);

    Task SaveChangesAsync(CancellationToken ct = default);
}
