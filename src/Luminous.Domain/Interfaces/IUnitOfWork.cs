namespace Luminous.Domain.Interfaces;

/// <summary>
/// Unit of work interface for coordinating transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the family repository.
    /// </summary>
    IFamilyRepository Families { get; }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the device repository.
    /// </summary>
    IDeviceRepository Devices { get; }

    /// <summary>
    /// Gets the event repository.
    /// </summary>
    IEventRepository Events { get; }

    /// <summary>
    /// Gets the chore repository.
    /// </summary>
    IChoreRepository Chores { get; }

    /// <summary>
    /// Gets the credential repository.
    /// </summary>
    ICredentialRepository Credentials { get; }

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
