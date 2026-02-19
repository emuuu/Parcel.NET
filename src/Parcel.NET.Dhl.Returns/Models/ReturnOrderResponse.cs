namespace Parcel.NET.Dhl.Returns.Models;

/// <summary>
/// Response from creating a DHL return order.
/// </summary>
public class ReturnOrderResponse
{
    /// <summary>
    /// Gets or sets the return shipment number.
    /// </summary>
    public required string ShipmentNo { get; set; }

    /// <summary>
    /// Gets or sets the international shipment number (if applicable).
    /// </summary>
    public string? InternationalShipmentNo { get; set; }

    /// <summary>
    /// Gets or sets the shipment label as Base64-encoded data (if requested).
    /// </summary>
    public string? LabelBase64 { get; set; }

    /// <summary>
    /// Gets or sets the QR label as Base64-encoded data (if requested).
    /// </summary>
    public string? QrLabelBase64 { get; set; }

    /// <summary>
    /// Gets or sets the routing code for the return shipment.
    /// </summary>
    public string? RoutingCode { get; set; }

    /// <summary>
    /// Gets or sets the status title from the API response.
    /// </summary>
    public string? StatusTitle { get; set; }

    /// <summary>
    /// Gets or sets the status detail from the API response.
    /// </summary>
    public string? StatusDetail { get; set; }
}
