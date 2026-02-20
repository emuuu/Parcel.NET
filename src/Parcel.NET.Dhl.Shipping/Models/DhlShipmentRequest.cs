using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// DHL-specific shipment creation request extending the carrier-agnostic <see cref="ShipmentRequest"/>.
/// </summary>
public class DhlShipmentRequest : ShipmentRequest
{
    /// <summary>
    /// Gets the DHL billing number (14-digit EKP + product + participation).
    /// </summary>
    public required string BillingNumber { get; init; }

    /// <summary>
    /// Gets the DHL business customer profile name. Required by the API.
    /// </summary>
    public required string Profile { get; init; }

    /// <summary>
    /// Gets the DHL shipping product. Defaults to <see cref="DhlProduct.V01PAK"/>.
    /// </summary>
    public DhlProduct Product { get; init; } = DhlProduct.V01PAK;

    /// <summary>
    /// Gets DHL-specific consignee delivery options (locker, post office, etc.).
    /// </summary>
    public DhlConsignee? DhlConsignee { get; init; }

    /// <summary>
    /// Gets optional value-added services for the shipment.
    /// </summary>
    public DhlValueAddedServices? ValueAddedServices { get; init; }

    /// <summary>
    /// Gets optional label generation options.
    /// </summary>
    public DhlLabelOptions? LabelOptions { get; init; }

    /// <summary>
    /// Gets optional customs details for international shipments.
    /// </summary>
    public DhlCustomsDetails? CustomsDetails { get; init; }

    /// <summary>
    /// Gets the optional cost center reference.
    /// </summary>
    public string? CostCenter { get; init; }

    /// <summary>
    /// Gets the optional creation software identifier.
    /// </summary>
    public string? CreationSoftware { get; init; }
}
