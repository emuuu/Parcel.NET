namespace Parcel.NET.Dhl.Pickup.Models;

/// <summary>
/// Response model returned when a pickup order is successfully created.
/// </summary>
public class PickupOrderResponse
{
    /// <summary>
    /// Gets or sets the DHL pickup order number.
    /// </summary>
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the confirmation status message.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the confirmed pickup date.
    /// </summary>
    public DateTimeOffset? ConfirmedPickupDate { get; set; }
}
