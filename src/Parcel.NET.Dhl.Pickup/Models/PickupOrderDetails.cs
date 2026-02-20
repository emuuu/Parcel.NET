namespace Parcel.NET.Dhl.Pickup.Models;

/// <summary>
/// Status of an existing pickup order, returned from GET /orders?orderID=...
/// </summary>
public class PickupOrderDetails
{
    /// <summary>
    /// Gets or sets the DHL pickup order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the current order state (e.g. "ANGENOMMEN", "STORNIERT").
    /// </summary>
    public required string OrderState { get; set; }

    /// <summary>
    /// Gets or sets the pickup date.
    /// </summary>
    public DateOnly? PickupDate { get; set; }
}

/// <summary>
/// A pickup location returned from GET /locations.
/// </summary>
public class PickupLocationInfo
{
    /// <summary>
    /// Gets or sets the agreed pickup location ID.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Gets or sets the address of the pickup location.
    /// </summary>
    public PickupAddress? Address { get; set; }
}
