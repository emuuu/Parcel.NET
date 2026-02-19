namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// A single tracking event in a shipment's history.
/// </summary>
public class TrackingEvent
{
    /// <summary>
    /// Gets the timestamp when this event occurred, or <c>null</c> when the carrier did not provide a timestamp.
    /// </summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>
    /// Gets the formatted location string (e.g. "Bonn, DE"), if available.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Gets a human-readable description of the event.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the carrier-specific status code for this event, if available.
    /// </summary>
    public string? StatusCode { get; init; }
}
