namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// DHL value-added services that can be added to a shipment.
/// </summary>
public class DhlValueAddedServices
{
    /// <summary>
    /// Gets the preferred delivery day in ISO 8601 format (e.g. "2026-02-20").
    /// </summary>
    public string? PreferredDay { get; init; }

    /// <summary>
    /// Gets the preferred drop-off location description (e.g. "Garage").
    /// </summary>
    public string? PreferredLocation { get; init; }

    /// <summary>
    /// Gets the preferred neighbour name for alternative delivery.
    /// </summary>
    public string? PreferredNeighbour { get; init; }

    /// <summary>
    /// Gets the cash on delivery amount, if applicable.
    /// </summary>
    public decimal? CashOnDeliveryAmount { get; init; }

    /// <summary>
    /// Gets the currency for <see cref="CashOnDeliveryAmount"/>. Defaults to EUR if not specified.
    /// </summary>
    public string? CashOnDeliveryCurrency { get; init; }

    /// <summary>
    /// Gets a value indicating whether additional insurance is requested.
    /// </summary>
    public bool AdditionalInsurance { get; init; }

    /// <summary>
    /// Gets a value indicating whether the shipment is classified as bulky goods.
    /// </summary>
    public bool BulkyGoods { get; init; }

    /// <summary>
    /// Gets a value indicating whether delivery is restricted to the named person only.
    /// </summary>
    public bool NamedPersonOnly { get; init; }

    /// <summary>
    /// Gets a value indicating whether neighbour delivery should be prevented.
    /// </summary>
    public bool NoNeighbourDelivery { get; init; }

    /// <summary>
    /// Gets the declared insured value amount, if applicable.
    /// </summary>
    public decimal? InsuredValue { get; init; }

    /// <summary>
    /// Gets the currency for <see cref="InsuredValue"/>. Defaults to EUR if not specified.
    /// </summary>
    public string? InsuredValueCurrency { get; init; }
}
