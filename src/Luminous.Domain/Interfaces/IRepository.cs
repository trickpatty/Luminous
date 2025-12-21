using Luminous.Domain.Common;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Base repository interface for read operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IReadRepository<T> where T : Entity
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<T?> GetByIdAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities matching the optional predicate.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities matching the optional partition key.
    /// </summary>
    Task<int> CountAsync(string? partitionKey = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base repository interface for write operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IWriteRepository<T> where T : Entity
{
    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts an entity (creates if not exists, updates if exists).
    /// </summary>
    Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    Task DeleteAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Combined repository interface for CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : Entity
{
}
