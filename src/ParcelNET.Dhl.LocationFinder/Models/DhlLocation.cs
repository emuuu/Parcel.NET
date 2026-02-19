namespace ParcelNET.Dhl.LocationFinder.Models;

/// <summary>
/// Represents a DHL service point location (Packstation, post office, parcel shop, etc.).
/// </summary>
public class DhlLocation
{
    /// <summary>
    /// Gets or sets the unique location ID.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the location type (e.g. "packstation", "postoffice", "parcelshop").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the distance from search point in meters (when searching by geo).
    /// </summary>
    public double? DistanceInMeters { get; set; }

    /// <summary>
    /// Gets or sets the opening hours as a formatted string.
    /// </summary>
    public string? OpeningHours { get; set; }

    /// <summary>
    /// Gets or sets the list of available services at this location.
    /// </summary>
    public List<string> Services { get; set; } = [];
}
