using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Information relevant to caregivers about a family member.
/// </summary>
public sealed class CaregiverInfo : ValueObject
{
    /// <summary>
    /// Gets or sets allergies and dietary restrictions.
    /// </summary>
    public List<string> Allergies { get; set; } = [];

    /// <summary>
    /// Gets or sets medical notes and conditions.
    /// </summary>
    public string? MedicalNotes { get; set; }

    /// <summary>
    /// Gets or sets emergency contact name.
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Gets or sets emergency contact phone number.
    /// </summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Gets or sets doctor/pediatrician name.
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// Gets or sets doctor/pediatrician phone number.
    /// </summary>
    public string? DoctorPhone { get; set; }

    /// <summary>
    /// Gets or sets school or daycare name.
    /// </summary>
    public string? SchoolName { get; set; }

    /// <summary>
    /// Gets or sets additional notes for caregivers.
    /// </summary>
    public string? Notes { get; set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return string.Join(",", Allergies);
        yield return MedicalNotes;
        yield return EmergencyContactName;
        yield return EmergencyContactPhone;
        yield return DoctorName;
        yield return DoctorPhone;
        yield return SchoolName;
        yield return Notes;
    }
}
