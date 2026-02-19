using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Abstractions;

/// <summary>
/// Carrier-agnostic interface for creating and cancelling shipments.
/// </summary>
public interface IShipmentService
{
    /// <summary>
    /// Creates a shipment with the specified carrier.
    /// </summary>
    /// <param name="request">The shipment creation request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The shipment response including shipment number and labels.</returns>
    /// <exception cref="Exceptions.ShippingException">Thrown when the carrier API returns an error.</exception>
    Task<ShipmentResponse> CreateShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a previously created shipment.
    /// </summary>
    /// <param name="shipmentNumber">The shipment number to cancel.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cancellation result indicating success or failure.</returns>
    Task<CancellationResult> CancelShipmentAsync(string shipmentNumber, CancellationToken cancellationToken = default);
}
