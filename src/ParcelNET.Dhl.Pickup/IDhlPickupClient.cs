using ParcelNET.Dhl.Pickup.Models;

namespace ParcelNET.Dhl.Pickup;

/// <summary>
/// Client interface for the DHL Parcel DE Pickup API v3.
/// </summary>
public interface IDhlPickupClient
{
    /// <summary>
    /// Creates a new pickup order.
    /// </summary>
    /// <param name="request">The pickup order request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The pickup order response with order number.</returns>
    Task<PickupOrderResponse> CreatePickupOrderAsync(PickupOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing pickup order.
    /// </summary>
    /// <param name="orderNumber">The pickup order number to cancel.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cancellation result.</returns>
    Task<PickupCancellationResult> CancelPickupOrderAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves details of an existing pickup order.
    /// </summary>
    /// <param name="orderNumber">The pickup order number.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The pickup order details.</returns>
    Task<PickupOrderDetails> GetPickupOrderAsync(string orderNumber, CancellationToken cancellationToken = default);
}
