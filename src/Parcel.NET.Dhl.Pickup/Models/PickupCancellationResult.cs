namespace Parcel.NET.Dhl.Pickup.Models;

/// <summary>
/// Result of a pickup order cancellation request (DELETE /orders?orderID=...).
/// </summary>
public class PickupCancellationResult
{
    /// <summary>
    /// Gets or sets the successfully cancelled orders.
    /// </summary>
    public IReadOnlyList<CancellationEntry> ConfirmedCancellations { get; set; } = [];

    /// <summary>
    /// Gets or sets the orders that failed to cancel.
    /// </summary>
    public IReadOnlyList<CancellationEntry> FailedCancellations { get; set; } = [];
}

/// <summary>
/// An individual cancellation entry (either confirmed or failed).
/// </summary>
public class CancellationEntry
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order state (e.g. "STORNIERT").
    /// </summary>
    public string? OrderState { get; set; }

    /// <summary>
    /// Gets or sets a status message.
    /// </summary>
    public string? Message { get; set; }
}
