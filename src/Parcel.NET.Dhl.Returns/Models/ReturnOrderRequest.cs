namespace Parcel.NET.Dhl.Returns.Models;

/// <summary>
/// Request model for creating a DHL return order.
/// </summary>
public class ReturnOrderRequest
{
    /// <summary>
    /// Gets or sets the receiver ID (DHL Retoure account identifier, e.g. "deu").
    /// </summary>
    public required string ReceiverId { get; set; }

    /// <summary>
    /// Gets or sets the customer reference (optional, max 30 characters).
    /// </summary>
    public string? CustomerReference { get; set; }

    /// <summary>
    /// Gets or sets the shipper (customer returning the parcel) address.
    /// </summary>
    public required ReturnShipper Shipper { get; set; }

    /// <summary>
    /// Gets or sets the label type to generate. Sent as a query parameter.
    /// Defaults to <see cref="ReturnLabelType.Both"/>.
    /// </summary>
    public ReturnLabelType LabelType { get; set; } = ReturnLabelType.Both;

    /// <summary>
    /// Gets or sets the item weight in grams (optional).
    /// </summary>
    public int? WeightInGrams { get; set; }

    /// <summary>
    /// Gets or sets the item value (optional).
    /// </summary>
    public ReturnItemValue? ItemValue { get; set; }

    /// <summary>
    /// Gets or sets customs details for non-EU returns (optional).
    /// </summary>
    public ReturnCustomsDetails? CustomsDetails { get; set; }
}

/// <summary>
/// Shipper address for a return order (the person returning the parcel).
/// </summary>
public class ReturnShipper
{
    /// <summary>
    /// Gets or sets the primary name line (required).
    /// </summary>
    public required string Name1 { get; set; }

    /// <summary>
    /// Gets or sets an additional name line (optional).
    /// </summary>
    public string? Name2 { get; set; }

    /// <summary>
    /// Gets or sets a third name line (optional).
    /// </summary>
    public string? Name3 { get; set; }

    /// <summary>
    /// Gets or sets the street name (required).
    /// </summary>
    public required string AddressStreet { get; set; }

    /// <summary>
    /// Gets or sets the house number (optional).
    /// </summary>
    public string? AddressHouse { get; set; }

    /// <summary>
    /// Gets or sets the postal code (required).
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city (required).
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-3 country code as a simple string (e.g. "deu").
    /// </summary>
    public required string Country { get; set; }

    /// <summary>
    /// Gets or sets the state or province (optional).
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets additional address information line 1 (optional).
    /// </summary>
    public string? AdditionalAddressInformation1 { get; set; }

    /// <summary>
    /// Gets or sets additional address information line 2 (optional).
    /// </summary>
    public string? AdditionalAddressInformation2 { get; set; }

    /// <summary>
    /// Gets or sets the email address (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number (optional).
    /// </summary>
    public string? Phone { get; set; }
}

/// <summary>
/// Monetary value with currency for a return item.
/// </summary>
public class ReturnItemValue
{
    /// <summary>
    /// Gets or sets the currency code (e.g. "EUR").
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Gets or sets the monetary value.
    /// </summary>
    public required decimal Value { get; set; }
}

/// <summary>
/// Customs details for non-EU return shipments.
/// </summary>
public class ReturnCustomsDetails
{
    /// <summary>
    /// Gets or sets the list of customs items.
    /// </summary>
    public required List<ReturnCustomsItem> Items { get; set; }
}

/// <summary>
/// A single customs item in a return shipment.
/// </summary>
public class ReturnCustomsItem
{
    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    public required string ItemDescription { get; set; }

    /// <summary>
    /// Gets or sets the quantity of items in the package.
    /// </summary>
    public required int PackagedQuantity { get; set; }

    /// <summary>
    /// Gets or sets the item weight in grams.
    /// </summary>
    public required int WeightInGrams { get; set; }

    /// <summary>
    /// Gets or sets the item value.
    /// </summary>
    public required ReturnItemValue ItemValue { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-3 country of origin (e.g. "deu").
    /// </summary>
    public required string CountryOfOrigin { get; set; }

    /// <summary>
    /// Gets or sets the HS tariff code (optional).
    /// </summary>
    public string? HsCode { get; set; }
}

/// <summary>
/// Type of return label to generate (sent as <c>labelType</c> query parameter).
/// </summary>
public enum ReturnLabelType
{
    /// <summary>Generate a shipment label.</summary>
    ShipmentLabel,

    /// <summary>Generate a QR label.</summary>
    QrLabel,

    /// <summary>Generate both shipment and QR label.</summary>
    Both
}
