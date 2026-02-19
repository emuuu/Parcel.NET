namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Response from a successful shipment creation.
/// </summary>
public class ShipmentResponse
{
    /// <summary>
    /// Gets the carrier-assigned shipment number.
    /// </summary>
    public required string ShipmentNumber { get; init; }

    /// <summary>
    /// Gets the list of shipping labels. May be empty if no labels were generated.
    /// </summary>
    public List<ShipmentLabel> Labels { get; init; } = [];

    /// <summary>
    /// Gets the URL for tracking the shipment online, if available.
    /// </summary>
    public string? TrackingUrl { get; init; }
}
