namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// Customs details for international DHL shipments.
/// </summary>
public class DhlCustomsDetails
{
    /// <summary>Gets the invoice number.</summary>
    public string? InvoiceNo { get; init; }

    /// <summary>Gets the export type (e.g. "COMMERCIAL_GOODS", "GIFT", "DOCUMENT", "COMMERCIAL_SAMPLE", "RETURN_OF_GOODS").</summary>
    public required string ExportType { get; init; }

    /// <summary>Gets the export description.</summary>
    public string? ExportDescription { get; init; }

    /// <summary>Gets the shipping conditions (Incoterms, e.g. "DAP", "DDP").</summary>
    public string? ShippingConditions { get; init; }

    /// <summary>Gets the postal charges amount.</summary>
    public decimal? PostalCharges { get; init; }

    /// <summary>Gets the postal charges currency. Defaults to EUR.</summary>
    public string PostalChargesCurrency { get; init; } = "EUR";

    /// <summary>Gets the customs items in the shipment.</summary>
    public required List<DhlCustomsItem> Items { get; init; }
}

/// <summary>
/// A single customs item in a shipment.
/// </summary>
public class DhlCustomsItem
{
    /// <summary>Gets the item description.</summary>
    public required string Description { get; init; }

    /// <summary>Gets the country of origin (ISO 3166-1 alpha-3).</summary>
    public required string CountryOfOrigin { get; init; }

    /// <summary>Gets the HS tariff code.</summary>
    public string? HsCode { get; init; }

    /// <summary>Gets the quantity.</summary>
    public int Quantity { get; init; } = 1;

    /// <summary>Gets the weight in kilograms.</summary>
    public double Weight { get; init; }

    /// <summary>Gets the declared value per item.</summary>
    public decimal Value { get; init; }

    /// <summary>Gets the value currency. Defaults to EUR.</summary>
    public string Currency { get; init; } = "EUR";
}
