namespace Parcel.NET.Dhl.Returns.Models;

/// <summary>
/// A DHL return drop-off location.
/// </summary>
public class ReturnLocation
{
    /// <summary>
    /// Gets or sets the location ID.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    public string? CountryCode { get; set; }
}
