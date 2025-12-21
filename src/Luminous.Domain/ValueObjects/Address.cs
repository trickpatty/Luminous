using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Represents a physical address or location.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Gets or sets the street address line 1.
    /// </summary>
    public string? Street1 { get; set; }

    /// <summary>
    /// Gets or sets the street address line 2.
    /// </summary>
    public string? Street2 { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets the formatted address string.
    /// </summary>
    public string FormattedAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Street1)) parts.Add(Street1);
            if (!string.IsNullOrEmpty(Street2)) parts.Add(Street2);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(State)) parts.Add(State);
            if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
            if (!string.IsNullOrEmpty(CountryCode)) parts.Add(CountryCode);
            return string.Join(", ", parts);
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street1;
        yield return Street2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return CountryCode;
        yield return Latitude;
        yield return Longitude;
    }
}
