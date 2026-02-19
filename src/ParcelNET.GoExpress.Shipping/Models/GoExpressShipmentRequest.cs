using ParcelNET.Abstractions.Models;

namespace ParcelNET.GoExpress.Shipping.Models;

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
}
