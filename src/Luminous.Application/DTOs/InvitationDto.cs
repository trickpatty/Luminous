using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for Invitation entity.
/// </summary>
public sealed record InvitationDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string Code { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public InvitationStatus Status { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public bool IsValid { get; init; }
    public DateTime? AcceptedAt { get; init; }
    public string? AcceptedUserId { get; init; }
}

/// <summary>
/// Request DTO for sending an invitation.
/// </summary>
public sealed record SendInvitationRequestDto
{
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.Adult;
    public string? Message { get; init; }
}

/// <summary>
/// Request DTO for accepting an invitation.
/// </summary>
public sealed record AcceptInvitationRequestDto
{
    public string DisplayName { get; init; } = string.Empty;
    public string? Nickname { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Color { get; init; }
}

/// <summary>
/// Response DTO for a newly accepted invitation (includes auth token).
/// </summary>
public sealed record AcceptedInvitationResultDto
{
    public UserDto User { get; init; } = new();
    public string AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
}

/// <summary>
/// Request DTO for updating a user's role.
/// </summary>
public sealed record UpdateUserRoleRequestDto
{
    public UserRole NewRole { get; init; }
}

/// <summary>
/// Request DTO for updating caregiver info.
/// </summary>
public sealed record UpdateCaregiverInfoRequestDto
{
    public List<string>? Allergies { get; init; }
    public string? MedicalNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? DoctorName { get; init; }
    public string? DoctorPhone { get; init; }
    public string? SchoolName { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for generating a caregiver access token.
/// </summary>
public sealed record GenerateCaregiverTokenRequestDto
{
    public int ExpirationHours { get; init; } = 24;
}

/// <summary>
/// Response DTO for a caregiver access token.
/// </summary>
public sealed record CaregiverAccessTokenDto
{
    public string Token { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string AccessUrl { get; init; } = string.Empty;
}
