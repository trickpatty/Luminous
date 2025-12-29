using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for User entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by ID within a family.
    /// </summary>
    new Task<User?> GetByIdAsync(string userId, string? familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID using a cross-partition query.
    /// Use this only when the familyId is not available (e.g., during passkey authentication).
    /// This is less efficient than the partition-key-based lookup.
    /// </summary>
    Task<User?> GetByIdCrossPartitionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by external identity provider ID.
    /// </summary>
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users in a family.
    /// </summary>
    Task<IReadOnlyList<User>> GetByFamilyIdAsync(string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already in use.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
