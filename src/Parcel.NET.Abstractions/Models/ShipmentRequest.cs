namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Carrier-agnostic shipment creation request.
/// </summary>
public class ShipmentRequest
{
    /// <summary>
    /// Gets the shipper (sender) address.
    /// </summary>
    public required Address Shipper { get; init; }

    /// <summary>
    /// Gets the consignee (recipient) address.
    /// </summary>
    public required Address Consignee { get; init; }

    /// <summary>
    /// Gets optional contact information for the shipper.
    /// </summary>
    public ContactInfo? ShipperContact { get; init; }

    /// <summary>
    /// Gets optional contact information for the consignee.
    /// </summary>
    public ContactInfo? ConsigneeContact { get; init; }

    /// <summary>
    /// Gets the list of packages to be shipped.
    /// </summary>
    public required List<Package> Packages { get; init; }

    /// <summary>
    /// Gets an optional customer reference for the shipment.
    /// </summary>
    public string? Reference { get; init; }

    /// <summary>
    /// Gets the planned ship date, if specified.
    /// </summary>
    public DateOnly? ShipDate { get; init; }
}
