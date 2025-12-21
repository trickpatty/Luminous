using Luminous.Domain.Common;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Events;

/// <summary>
/// Event raised when a new user is created.
/// </summary>
public sealed class UserCreatedEvent : DomainEvent
{
    public string UserId { get; }
    public string FamilyId { get; }
    public string Email { get; }
    public UserRole Role { get; }

    public UserCreatedEvent(string userId, string familyId, string email, UserRole role)
    {
        UserId = userId;
        FamilyId = familyId;
        Email = email;
        Role = role;
    }
}

/// <summary>
/// Event raised when a user's profile is updated.
/// </summary>
public sealed class UserProfileUpdatedEvent : DomainEvent
{
    public string UserId { get; }
    public string FamilyId { get; }

    public UserProfileUpdatedEvent(string userId, string familyId)
    {
        UserId = userId;
        FamilyId = familyId;
    }
}

/// <summary>
/// Event raised when a user's role is changed.
/// </summary>
public sealed class UserRoleChangedEvent : DomainEvent
{
    public string UserId { get; }
    public string FamilyId { get; }
    public UserRole OldRole { get; }
    public UserRole NewRole { get; }
    public string ChangedBy { get; }

    public UserRoleChangedEvent(string userId, string familyId, UserRole oldRole, UserRole newRole, string changedBy)
    {
        UserId = userId;
        FamilyId = familyId;
        OldRole = oldRole;
        NewRole = newRole;
        ChangedBy = changedBy;
    }
}

/// <summary>
/// Event raised when a user logs in.
/// </summary>
public sealed class UserLoggedInEvent : DomainEvent
{
    public string UserId { get; }
    public string FamilyId { get; }
    public string AuthMethod { get; }

    public UserLoggedInEvent(string userId, string familyId, string authMethod)
    {
        UserId = userId;
        FamilyId = familyId;
        AuthMethod = authMethod;
    }
}
