using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Shipping.Models;

namespace ParcelNET.Dhl.Shipping;

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
    /// Creates a daily closing manifest for all unmanifested shipments.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The manifest creation result.</returns>
    Task<ManifestResult> CreateManifestAsync(CancellationToken cancellationToken = default);
}
