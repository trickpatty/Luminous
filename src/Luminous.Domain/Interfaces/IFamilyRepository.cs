using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Family entities.
/// </summary>
public interface IFamilyRepository : IRepository<Family>
{
    /// <summary>
    /// Gets a family by its ID.
    /// </summary>
    Task<Family?> GetByIdAsync(string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a family name is already in use.
    /// </summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}
