namespace Parcel.NET.Dhl.Pickup.Models;

/// <summary>
/// Request model for creating a DHL pickup order via POST /orders.
/// </summary>
public class PickupOrderRequest
{
    /// <summary>
    /// Gets or sets the DHL billing number (EKP + product/procedure code).
    /// Pattern: \d{10}\d{2}\w{2}. Required.
    /// </summary>
    public required string BillingNumber { get; set; }

    /// <summary>
    /// Gets or sets the pickup location. Either an address or a location ID must be provided.
    /// </summary>
    public required PickupLocation Location { get; set; }

    /// <summary>
    /// Gets or sets the business hours at the pickup location.
    /// </summary>
    public IReadOnlyList<PickupTimeFrame>? BusinessHours { get; set; }

    /// <summary>
    /// Gets or sets the contact persons for the pickup (0-2 allowed).
    /// </summary>
    public IReadOnlyList<PickupContactPerson>? ContactPersons { get; set; }

    /// <summary>
    /// Gets or sets the pickup date. Required.
    /// </summary>
    public required DateOnly PickupDate { get; set; }

    /// <summary>
    /// Gets or sets whether to use ASAP scheduling instead of a fixed date.
    /// When true, <see cref="PickupDate"/> is ignored and the API schedules ASAP.
    /// </summary>
    public bool UseAsapScheduling { get; set; }

    /// <summary>
    /// Gets or sets the total weight of all shipments in kilograms.
    /// </summary>
    public double? TotalWeightInKg { get; set; }

    /// <summary>
    /// Gets or sets an optional comment for the pickup (max 100 characters).
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the shipment details. At least one shipment is required.
    /// </summary>
    public required IReadOnlyList<PickupShipment> Shipments { get; set; }
}

/// <summary>
/// Pickup location — either an address or a known location ID.
/// </summary>
public class PickupLocation
{
    /// <summary>
    /// Gets or sets the pickup address. Provide this OR <see cref="LocationId"/>.
    /// </summary>
    public PickupAddress? Address { get; set; }

    /// <summary>
    /// Gets or sets the agreed pickup location ID (e.g. "AS3254120698").
    /// Provide this OR <see cref="Address"/>.
    /// </summary>
    public string? LocationId { get; set; }
}

/// <summary>
/// Address details for a pickup location.
/// </summary>
public class PickupAddress
{
    /// <summary>
    /// Gets or sets the primary name (company or person). Max 50 characters.
    /// </summary>
    public required string Name1 { get; set; }

    /// <summary>
    /// Gets or sets the secondary name line. Max 50 characters.
    /// </summary>
    public string? Name2 { get; set; }

    /// <summary>
    /// Gets or sets the street name. Max 70 characters.
    /// </summary>
    public required string Street { get; set; }

    /// <summary>
    /// Gets or sets the house number. Max 10 characters.
    /// </summary>
    public required string HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code. Max 7 characters.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city name. Max 35 characters.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 country code (e.g. "DE").
    /// </summary>
    public string Country { get; set; } = "DE";

    /// <summary>
    /// Gets or sets the state/province.
    /// </summary>
    public string? State { get; set; }
}

/// <summary>
/// A business hours time frame.
/// </summary>
public class PickupTimeFrame
{
    /// <summary>
    /// Gets or sets the start time (e.g. "08:00").
    /// </summary>
    public required string TimeFrom { get; set; }

    /// <summary>
    /// Gets or sets the end time (e.g. "17:00").
    /// </summary>
    public required string TimeUntil { get; set; }
}

/// <summary>
/// Contact person details for a pickup order.
/// </summary>
public class PickupContactPerson
{
    /// <summary>
    /// Gets or sets the contact person's name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the contact email address.
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// Individual shipment within a pickup order.
/// </summary>
public class PickupShipment
{
    /// <summary>
    /// Gets or sets the transportation type. Required.
    /// Valid values: PAKET, ROLLBEHAELTER, WECHSELBEHAELTER, PALETTEN, SPERRGUT.
    /// </summary>
    public required string TransportationType { get; set; }

    /// <summary>
    /// Gets or sets whether this is a replacement shipment.
    /// </summary>
    public bool? Replacement { get; set; }

    /// <summary>
    /// Gets or sets the shipment number.
    /// </summary>
    public string? ShipmentNo { get; set; }

    /// <summary>
    /// Gets or sets the shipment size. Valid values: S, M, L.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets whether this is a bulky good.
    /// </summary>
    public bool? BulkyGood { get; set; }

    /// <summary>
    /// Gets or sets whether to request label printing.
    /// </summary>
    public bool? PrintLabel { get; set; }

    /// <summary>
    /// Gets or sets a customer reference for this shipment.
    /// </summary>
    public string? CustomerReference { get; set; }
}
