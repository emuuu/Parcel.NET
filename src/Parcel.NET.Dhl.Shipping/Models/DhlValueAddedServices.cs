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
    /// Gets the cash-on-delivery details including amount and bank account.
    /// </summary>
    public DhlCashOnDelivery? CashOnDelivery { get; init; }

    /// <summary>
    /// Gets the declared insured value amount, if applicable.
    /// </summary>
    public decimal? InsuredValue { get; init; }

    /// <summary>
    /// Gets the currency for <see cref="InsuredValue"/>. Defaults to EUR if not specified.
    /// </summary>
    public string? InsuredValueCurrency { get; init; }

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
    /// Gets a value indicating whether the recipient must sign for the shipment.
    /// </summary>
    public bool SignedForByRecipient { get; init; }

    /// <summary>
    /// Gets a value indicating whether premium delivery is requested.
    /// </summary>
    public bool Premium { get; init; }

    /// <summary>
    /// Gets a value indicating whether to deliver to the closest drop point.
    /// </summary>
    public bool ClosestDropPoint { get; init; }

    /// <summary>
    /// Gets a value indicating whether postal delivery duty paid is requested.
    /// </summary>
    public bool PostalDeliveryDutyPaid { get; init; }

    /// <summary>
    /// Gets the endorsement type for international shipments: "RETURN" or "ABANDON".
    /// </summary>
    public string? Endorsement { get; init; }

    /// <summary>
    /// Gets the visual check of age requirement: "A16" or "A18".
    /// </summary>
    public string? VisualCheckOfAge { get; init; }

    /// <summary>
    /// Gets the parcel outlet routing email address.
    /// </summary>
    public string? ParcelOutletRouting { get; init; }

    /// <summary>
    /// Gets ident check details for identity verification on delivery.
    /// </summary>
    public DhlIdentCheck? IdentCheck { get; init; }

    /// <summary>
    /// Gets DHL retoure (return label) details.
    /// </summary>
    public DhlRetoureService? DhlRetoure { get; init; }
}

/// <summary>
/// Cash-on-delivery service details.
/// </summary>
public class DhlCashOnDelivery
{
    /// <summary>
    /// Gets the COD amount to collect.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Gets the currency. Defaults to EUR.
    /// </summary>
    public string Currency { get; init; } = "EUR";

    /// <summary>
    /// Gets the bank account holder name.
    /// </summary>
    public string? AccountHolder { get; init; }

    /// <summary>
    /// Gets the IBAN.
    /// </summary>
    public string? Iban { get; init; }

    /// <summary>
    /// Gets the BIC (optional).
    /// </summary>
    public string? Bic { get; init; }

    /// <summary>
    /// Gets the account reference.
    /// </summary>
    public string? AccountReference { get; init; }

    /// <summary>
    /// Gets the transfer note line 1.
    /// </summary>
    public string? TransferNote1 { get; init; }

    /// <summary>
    /// Gets the transfer note line 2.
    /// </summary>
    public string? TransferNote2 { get; init; }
}

/// <summary>
/// Identity check service details.
/// </summary>
public class DhlIdentCheck
{
    /// <summary>Gets the recipient's first name.</summary>
    public required string FirstName { get; init; }

    /// <summary>Gets the recipient's last name.</summary>
    public required string LastName { get; init; }

    /// <summary>Gets the recipient's date of birth (yyyy-MM-dd).</summary>
    public required string DateOfBirth { get; init; }

    /// <summary>Gets the minimum age requirement: "A16" or "A18".</summary>
    public string? MinimumAge { get; init; }
}

/// <summary>
/// DHL retoure (return label) service details.
/// </summary>
public class DhlRetoureService
{
    /// <summary>Gets the billing number for the return shipment.</summary>
    public required string BillingNumber { get; init; }
}
