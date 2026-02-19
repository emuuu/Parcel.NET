namespace Parcel.NET.Dhl.Pickup.Models;

/// <summary>
/// Response model returned when a pickup order is successfully created via POST /orders.
/// </summary>
public class PickupOrderResponse
{
    /// <summary>
    /// Gets or sets the DHL pickup order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the confirmed pickup date.
    /// </summary>
    public DateOnly? PickupDate { get; set; }

    /// <summary>
    /// Gets or sets whether the pickup is free of charge.
    /// </summary>
    public bool FreeOfCharge { get; set; }

    /// <summary>
    /// Gets or sets the pickup type (e.g. "BDA").
    /// </summary>
    public string? PickupType { get; set; }

    /// <summary>
    /// Gets or sets the confirmation type (e.g. "ORDERPICKUP").
    /// </summary>
    public string? ConfirmationType { get; set; }

    /// <summary>
    /// Gets or sets the confirmed shipments.
    /// </summary>
    public IReadOnlyList<ConfirmedShipment> ConfirmedShipments { get; set; } = [];
}

/// <summary>
/// A shipment confirmed as part of a pickup order.
/// </summary>
public class ConfirmedShipment
{
    /// <summary>
    /// Gets or sets the transportation type (e.g. "PAKET").
    /// </summary>
    public string? TransportationType { get; set; }

    /// <summary>
    /// Gets or sets the shipment number.
    /// </summary>
    public string? ShipmentNo { get; set; }

    /// <summary>
    /// Gets or sets the order date/time.
    /// </summary>
    public DateTimeOffset? OrderDate { get; set; }
}
