using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.GoExpress.Shipping.Models;

/// <summary>
/// GO! Express-specific shipment request extending the carrier-agnostic <see cref="ShipmentRequest"/>.
/// </summary>
public class GoExpressShipmentRequest : ShipmentRequest
{
    /// <summary>
    /// Gets or sets the GO! Express service type (e.g. ON, INT, LET).
    /// </summary>
    public required GoExpressService Service { get; init; }

    /// <summary>
    /// Gets or sets the pickup time window.
    /// </summary>
    public required TimeWindow Pickup { get; init; }

    /// <summary>
    /// Gets or sets the optional delivery time window.
    /// </summary>
    public TimeWindow? Delivery { get; init; }

    /// <summary>
    /// Gets or sets whether the sender delivers to the GO! station (self-pickup by GO!).
    /// </summary>
    public bool SelfPickup { get; init; }

    /// <summary>
    /// Gets or sets whether the recipient picks up at the GO! station (self-delivery).
    /// </summary>
    public bool SelfDelivery { get; init; }

    /// <summary>
    /// Gets or sets whether the recipient pays the freight.
    /// </summary>
    public bool FreightCollect { get; init; }

    /// <summary>
    /// Gets or sets whether identity verification is required at delivery.
    /// </summary>
    public bool IdentCheck { get; init; }

    /// <summary>
    /// Gets or sets whether a receipt notice is required.
    /// </summary>
    public bool ReceiptNotice { get; init; }

    /// <summary>
    /// Gets or sets whether this is a neutral pickup (consignor address hidden from consignee).
    /// </summary>
    public bool IsNeutralPickup { get; init; }

    /// <summary>
    /// Gets or sets the neutral address (used when <see cref="IsNeutralPickup"/> is true).
    /// </summary>
    public Address? NeutralAddress { get; init; }

    /// <summary>
    /// Gets or sets optional contact information for the neutral address.
    /// </summary>
    public ContactInfo? NeutralAddressContact { get; init; }

    /// <summary>
    /// Gets or sets the insurance amount.
    /// </summary>
    public decimal? InsuranceAmount { get; init; }

    /// <summary>
    /// Gets or sets the insurance currency (e.g. "EUR").
    /// </summary>
    public string? InsuranceCurrency { get; init; }

    /// <summary>
    /// Gets or sets the cash on delivery amount.
    /// </summary>
    public decimal? CashOnDeliveryAmount { get; init; }

    /// <summary>
    /// Gets or sets the cash on delivery currency (e.g. "EUR").
    /// </summary>
    public string? CashOnDeliveryCurrency { get; init; }

    /// <summary>
    /// Gets or sets the declared value of goods.
    /// </summary>
    public decimal? ValueOfGoodsAmount { get; init; }

    /// <summary>
    /// Gets or sets the value of goods currency (e.g. "EUR").
    /// </summary>
    public string? ValueOfGoodsCurrency { get; init; }

    /// <summary>
    /// Gets or sets an optional cost center reference.
    /// </summary>
    public string? CostCenter { get; init; }

    /// <summary>
    /// Gets or sets the shipment content description.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Gets or sets the desired label format. Defaults to <see cref="GoExpressLabelFormat.PdfA4"/>.
    /// </summary>
    public GoExpressLabelFormat LabelFormat { get; init; } = GoExpressLabelFormat.PdfA4;

    /// <summary>
    /// Gets or sets free-form dimensions string (e.g. "120x80x60").
    /// </summary>
    public string? Dimensions { get; init; }

    /// <summary>
    /// Gets or sets remarks for the shipper address.
    /// </summary>
    public string? ShipperRemarks { get; init; }

    /// <summary>
    /// Gets or sets remarks for the consignee address.
    /// </summary>
    public string? ConsigneeRemarks { get; init; }

    /// <summary>
    /// Gets or sets whether telephone avis is requested for the shipper.
    /// </summary>
    public bool ShipperTelephoneAvis { get; init; }

    /// <summary>
    /// Gets or sets whether telephone avis is requested for the consignee.
    /// </summary>
    public bool ConsigneeTelephoneAvis { get; init; }

    /// <summary>
    /// Gets or sets the delivery code for the consignee.
    /// </summary>
    public string? ConsigneeDeliveryCode { get; init; }

    /// <summary>
    /// Gets or sets whether the consignee delivery code should be encrypted.
    /// </summary>
    public bool ConsigneeDeliveryCodeEncryption { get; init; }
}
