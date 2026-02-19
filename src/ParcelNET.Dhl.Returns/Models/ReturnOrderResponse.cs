namespace ParcelNET.Dhl.Returns.Models;

/// <summary>
/// Response from creating a DHL return order.
/// </summary>
public class ReturnOrderResponse
{
    /// <summary>
    /// Gets or sets the return shipment number.
    /// </summary>
    public required string ShipmentNumber { get; set; }

    /// <summary>
    /// Gets or sets the return label as Base64-encoded PDF (if available).
    /// </summary>
    public string? LabelPdf { get; set; }

    /// <summary>
    /// Gets or sets the return label URL (if available).
    /// </summary>
    public string? LabelUrl { get; set; }

    /// <summary>
    /// Gets or sets the QR code as Base64-encoded image (if available).
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// Gets or sets the routing code for the return shipment.
    /// </summary>
    public string? RoutingCode { get; set; }
}
