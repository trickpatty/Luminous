using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Nanoid;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents an invitation to join a family.
/// </summary>
public sealed class Invitation : Entity
{
    /// <summary>
    /// Gets or sets the family ID (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the invitee.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role assigned to the invitee.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Adult;

    /// <summary>
    /// Gets or sets the unique invitation code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the invitation status.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// Gets or sets when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID created upon acceptance.
    /// </summary>
    public string? AcceptedUserId { get; set; }

    /// <summary>
    /// Gets or sets a personal message to include in the invitation.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets whether this invitation is still valid.
    /// </summary>
    public bool IsValid => Status == InvitationStatus.Pending && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Creates a new invitation.
    /// </summary>
    public static Invitation Create(
        string familyId,
        string email,
        UserRole role,
        string invitedBy,
        int expirationDays = 7,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        return new Invitation
        {
            FamilyId = familyId,
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Code = GenerateCode(),
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            Message = message,
            CreatedBy = invitedBy
        };
    }

    /// <summary>
    /// Accepts this invitation.
    /// </summary>
    public void Accept(string userId)
    {
        if (!IsValid)
            throw new InvalidOperationException("Invitation is no longer valid.");

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        AcceptedUserId = userId;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Declines this invitation.
    /// </summary>
    public void Decline()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Can only decline pending invitations.");

        Status = InvitationStatus.Declined;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes this invitation.
    /// </summary>
    public void Revoke(string revokedBy)
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Can only revoke pending invitations.");

        Status = InvitationStatus.Revoked;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = revokedBy;
    }

    /// <summary>
    /// Generates a unique invitation code.
    /// Uses NanoId for URL-friendly, compact unique identifiers.
    /// </summary>
    private static string GenerateCode()
    {
        return Nanoid.Generate(size: 22);
    }
}
