using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Shipping.Models;

namespace Parcel.NET.Dhl.Shipping;

/// <summary>
/// DHL-specific shipping client extending the carrier-agnostic <see cref="IShipmentService"/>
/// with additional DHL operations.
/// </summary>
public interface IDhlShippingClient : IShipmentService
{
    /// <summary>
    /// Validates a shipment request against the DHL API without creating a shipment.
    /// </summary>
    /// <param name="request">The shipment request to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The validation result with any warnings or errors.</returns>
    Task<ValidationResult> ValidateShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a shipment with a specific DHL profile.
    /// </summary>
    /// <param name="shipmentNumber">The shipment number to cancel.</param>
    /// <param name="profile">The DHL business customer profile name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cancellation result.</returns>
    Task<CancellationResult> CancelShipmentAsync(string shipmentNumber, string profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a daily closing manifest for all unmanifested shipments using the default profile.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The manifest creation result.</returns>
    Task<ManifestResult> CreateManifestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a daily closing manifest with specific options.
    /// </summary>
    /// <param name="profile">The DHL business customer profile name.</param>
    /// <param name="shipmentNumbers">Optional list of specific shipment numbers to manifest.</param>
    /// <param name="billingNumber">Optional billing number filter.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The manifest creation result.</returns>
    Task<ManifestResult> CreateManifestAsync(string profile, List<string>? shipmentNumbers = null, string? billingNumber = null, CancellationToken cancellationToken = default);
}
