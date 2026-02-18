namespace ParcelNET.Abstractions.Models;

/// <summary>
/// Result of a shipment tracking request.
/// </summary>
public class TrackingResult
{
    /// <summary>
    /// Gets the shipment number.
    /// </summary>
    public required string ShipmentNumber { get; init; }

    /// <summary>
    /// Gets the current overall tracking status.
    /// </summary>
    public TrackingStatus Status { get; init; }

    /// <summary>
    /// Gets the chronological list of tracking events. May be empty.
    /// </summary>
    public List<TrackingEvent> Events { get; init; } = [];

    /// <summary>
    /// Gets the estimated delivery date and time, if available.
    /// </summary>
    public DateTimeOffset? EstimatedDelivery { get; init; }
}
