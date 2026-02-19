namespace ParcelNET.Dhl.Pickup.Models;

/// <summary>
/// Full details of an existing pickup order.
/// </summary>
public class PickupOrderDetails
{
    /// <summary>
    /// Gets or sets the DHL pickup order number.
    /// </summary>
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the current status of the pickup order
    /// (e.g. "CONFIRMED", "CANCELLED", "COMPLETED").
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the pickup address.
    /// </summary>
    public required PickupAddress Address { get; set; }

    /// <summary>
    /// Gets or sets the contact details.
    /// </summary>
    public required PickupContact Contact { get; set; }

    /// <summary>
    /// Gets or sets the start of the pickup time window.
    /// </summary>
    public required DateTimeOffset PickupFrom { get; set; }

    /// <summary>
    /// Gets or sets the end of the pickup time window.
    /// </summary>
    public required DateTimeOffset PickupUntil { get; set; }

    /// <summary>
    /// Gets or sets the number of packages.
    /// </summary>
    public int PackageCount { get; set; }

    /// <summary>
    /// Gets or sets the total weight in kilograms.
    /// </summary>
    public double TotalWeightInKg { get; set; }

    /// <summary>
    /// Gets or sets the customer reference, if any.
    /// </summary>
    public string? CustomerReference { get; set; }
}
