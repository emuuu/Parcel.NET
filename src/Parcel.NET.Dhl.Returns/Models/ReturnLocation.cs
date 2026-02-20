namespace Parcel.NET.Dhl.Returns.Models;

/// <summary>
/// A DHL return location (drop-off point).
/// </summary>
public class ReturnLocation
{
    /// <summary>
    /// Gets or sets the receiver ID for this location.
    /// </summary>
    public required string ReceiverId { get; set; }

    /// <summary>
    /// Gets or sets the billing number associated with this location.
    /// </summary>
    public string? BillingNumber { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string? Name { get; set; }

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
    /// Gets or sets the country code.
    /// </summary>
    public string? Country { get; set; }
}
