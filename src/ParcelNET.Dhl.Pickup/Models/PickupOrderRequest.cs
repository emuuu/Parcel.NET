namespace ParcelNET.Dhl.Pickup.Models;

/// <summary>
/// Request model for creating a DHL pickup order.
/// </summary>
public class PickupOrderRequest
{
    /// <summary>
    /// Gets or sets the pickup address.
    /// </summary>
    public required PickupAddress Address { get; set; }

    /// <summary>
    /// Gets or sets the contact person for the pickup.
    /// </summary>
    public required PickupContact Contact { get; set; }

    /// <summary>
    /// Gets or sets the earliest pickup date and time.
    /// </summary>
    public required DateTimeOffset PickupFrom { get; set; }

    /// <summary>
    /// Gets or sets the latest pickup date and time.
    /// </summary>
    public required DateTimeOffset PickupUntil { get; set; }

    /// <summary>
    /// Gets or sets the total number of packages to be picked up.
    /// </summary>
    public required int PackageCount { get; set; }

    /// <summary>
    /// Gets or sets the total weight of all packages in kilograms.
    /// </summary>
    public required double TotalWeightInKg { get; set; }

    /// <summary>
    /// Gets or sets an optional customer reference for the pickup order.
    /// </summary>
    public string? CustomerReference { get; set; }

    /// <summary>
    /// Gets or sets optional remarks or instructions for the driver.
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the DHL billing number (EKP with product/procedure code).
    /// </summary>
    public string? BillingNumber { get; set; }
}

/// <summary>
/// Address details for a pickup location.
/// </summary>
public class PickupAddress
{
    /// <summary>
    /// Gets or sets the company or person name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public required string Street { get; set; }

    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public required string HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 country code (e.g. "DE").
    /// </summary>
    public string Country { get; set; } = "DE";

    /// <summary>
    /// Gets or sets an optional additional address line.
    /// </summary>
    public string? AddressAddition { get; set; }
}

/// <summary>
/// Contact details for a pickup order.
/// </summary>
public class PickupContact
{
    /// <summary>
    /// Gets or sets the contact person's name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number.
    /// </summary>
    public required string Phone { get; set; }

    /// <summary>
    /// Gets or sets the contact email address.
    /// </summary>
    public string? Email { get; set; }
}
