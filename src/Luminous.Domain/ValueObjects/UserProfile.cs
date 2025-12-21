using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Profile information for a user.
/// </summary>
public sealed class UserProfile : ValueObject
{
    /// <summary>
    /// Gets or sets the user's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's profile color (hex code).
    /// </summary>
    public string Color { get; set; } = "#3B82F6";

    /// <summary>
    /// Gets or sets the user's birthday.
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// Gets or sets the user's nickname.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets whether to show age on profile.
    /// </summary>
    public bool ShowAge { get; set; }

    /// <summary>
    /// Gets the user's current age if birthday is set.
    /// </summary>
    public int? Age
    {
        get
        {
            if (Birthday == null) return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - Birthday.Value.Year;

            if (Birthday.Value > today.AddYears(-age))
                age--;

            return age;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AvatarUrl;
        yield return Color;
        yield return Birthday;
        yield return Nickname;
        yield return ShowAge;
    }
}
