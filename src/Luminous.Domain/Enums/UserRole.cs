namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the roles a user can have within a family.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Family owner with full access including billing and family deletion.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Administrator with full access except billing management.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Adult family member with full feature access.
    /// </summary>
    Adult = 2,

    /// <summary>
    /// Teenager with limited access to certain features.
    /// </summary>
    Teen = 3,

    /// <summary>
    /// Child with view and complete-only access.
    /// </summary>
    Child = 4,

    /// <summary>
    /// External caregiver with view-only access.
    /// </summary>
    Caregiver = 5
}
