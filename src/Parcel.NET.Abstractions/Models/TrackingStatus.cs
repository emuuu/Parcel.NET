namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Carrier-agnostic shipment tracking status.
/// </summary>
public enum TrackingStatus
{
    /// <summary>Status could not be determined.</summary>
    Unknown,
    /// <summary>Label created but not yet handed over to carrier.</summary>
    PreTransit,
    /// <summary>Shipment is in transit.</summary>
    InTransit,
    /// <summary>Shipment is out for delivery.</summary>
    OutForDelivery,
    /// <summary>Shipment has been delivered.</summary>
    Delivered,
    /// <summary>An exception or problem occurred during delivery.</summary>
    Exception,
    /// <summary>Shipment has been returned to sender.</summary>
    Returned
}
