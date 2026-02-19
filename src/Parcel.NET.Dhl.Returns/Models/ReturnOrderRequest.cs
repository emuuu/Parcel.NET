namespace Parcel.NET.Dhl.Returns.Models;

/// <summary>
/// Request model for creating a DHL return order.
/// </summary>
public class ReturnOrderRequest
{
    /// <summary>
    /// Gets or sets the receiver ID (DHL Retoure account number).
    /// </summary>
    public required string ReceiverId { get; set; }

    /// <summary>
    /// Gets or sets the sender (customer returning the parcel) address.
    /// </summary>
    public required ReturnAddress SenderAddress { get; set; }

    /// <summary>
    /// Gets or sets the shipment reference (optional, for sender's reference).
    /// </summary>
    public string? ShipmentReference { get; set; }

    /// <summary>
    /// Gets or sets the return document type. Defaults to <see cref="ReturnDocumentType.Both"/>.
    /// </summary>
    public ReturnDocumentType DocumentType { get; set; } = ReturnDocumentType.Both;

    /// <summary>
    /// Gets or sets the email address for notifications (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number for notifications (optional).
    /// </summary>
    public string? TelephoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the weight in grams (optional).
    /// </summary>
    public int? WeightInGrams { get; set; }

    /// <summary>
    /// Gets or sets the monetary value of the return (optional).
    /// </summary>
    public decimal? Value { get; set; }
}

/// <summary>
/// Address for a return order sender.
/// </summary>
public class ReturnAddress
{
    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public required string Street { get; set; }

    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-3 country code.
    /// </summary>
    public required string CountryCode { get; set; }
}

/// <summary>
/// Type of return document to generate.
/// </summary>
public enum ReturnDocumentType
{
    /// <summary>Generate both PDF label and QR code.</summary>
    Both,
    /// <summary>Generate only the PDF label URL.</summary>
    Url,
    /// <summary>Generate only the QR code.</summary>
    Qr
}
