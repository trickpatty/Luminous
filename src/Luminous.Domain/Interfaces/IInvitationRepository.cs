using Luminous.Domain.Entities;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Invitation entities.
/// </summary>
public interface IInvitationRepository : IRepository<Invitation>
{
    /// <summary>
    /// Gets an invitation by ID within a family.
    /// </summary>
    new Task<Invitation?> GetByIdAsync(string invitationId, string? familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by its unique code.
    /// </summary>
    Task<Invitation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invitations for a family.
    /// </summary>
    Task<IReadOnlyList<Invitation>> GetByFamilyIdAsync(string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets invitations for a family filtered by status.
    /// </summary>
    Task<IReadOnlyList<Invitation>> GetByFamilyIdAndStatusAsync(
        string familyId,
        InvitationStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invitation for a specific email within a family.
    /// </summary>
    Task<Invitation?> GetPendingByEmailAndFamilyIdAsync(
        string email,
        string familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a pending invitation exists for an email within a family.
    /// </summary>
    Task<bool> HasPendingInvitationAsync(
        string email,
        string familyId,
        CancellationToken cancellationToken = default);
}
