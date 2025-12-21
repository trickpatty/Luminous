using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for User entity.
/// </summary>
public sealed record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public UserProfileDto Profile { get; init; } = new();
    public CaregiverInfoDto? CaregiverInfo { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Data transfer object for UserProfile.
/// </summary>
public sealed record UserProfileDto
{
    public string? AvatarUrl { get; init; }
    public string Color { get; init; } = "#3B82F6";
    public string? Birthday { get; init; }
    public string? Nickname { get; init; }
    public bool ShowAge { get; init; }
    public int? Age { get; init; }
}

/// <summary>
/// Data transfer object for CaregiverInfo.
/// </summary>
public sealed record CaregiverInfoDto
{
    public List<string> Allergies { get; init; } = [];
    public string? MedicalNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? DoctorName { get; init; }
    public string? DoctorPhone { get; init; }
    public string? SchoolName { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Simplified user reference DTO.
/// </summary>
public sealed record UserRefDto
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string Color { get; init; } = "#3B82F6";
}
