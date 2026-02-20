using Parcel.NET.Dhl.Pickup.Models;

namespace Parcel.NET.Dhl.Pickup;

/// <summary>
/// Client interface for the DHL Parcel DE Pickup API v3.
/// </summary>
public interface IDhlPickupClient
{
    /// <summary>
    /// Creates a new pickup order (POST /orders).
    /// </summary>
    /// <param name="request">The pickup order request details.</param>
    /// <param name="validateOnly">When true, the order is only validated but not created.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The pickup order response with order ID and confirmed shipments.</returns>
    Task<PickupOrderResponse> CreatePickupOrderAsync(
        PickupOrderRequest request,
        bool validateOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels one or more existing pickup orders (DELETE /orders?orderID=...).
    /// </summary>
    /// <param name="orderIds">The pickup order IDs to cancel (1-100).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cancellation result with confirmed and failed cancellations.</returns>
    Task<PickupCancellationResult> CancelPickupOrdersAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status of one or more pickup orders (GET /orders?orderID=...).
    /// </summary>
    /// <param name="orderIds">The pickup order IDs to query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of pickup order status objects.</returns>
    Task<IReadOnlyList<PickupOrderDetails>> GetPickupOrdersAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves available pickup locations (GET /locations).
    /// </summary>
    /// <param name="postalCode">Optional postal code filter.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of available pickup locations.</returns>
    Task<IReadOnlyList<PickupLocationInfo>> GetPickupLocationsAsync(
        string? postalCode = null,
        CancellationToken cancellationToken = default);
}
