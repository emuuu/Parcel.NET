using Parcel.NET.Dhl.Returns.Models;

namespace Parcel.NET.Dhl.Returns;

/// <summary>
/// Client interface for the DHL Parcel DE Returns API v1.
/// </summary>
public interface IDhlReturnsClient
{
    /// <summary>
    /// Creates a new return order and generates a return label.
    /// The <see cref="ReturnOrderRequest.LabelType"/> is sent as the <c>labelType</c> query parameter.
    /// </summary>
    /// <param name="request">The return order request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The return order response with shipment number and label data.</returns>
    Task<ReturnOrderResponse> CreateReturnOrderAsync(ReturnOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves available return locations (drop-off points).
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-3 country code (required).</param>
    /// <param name="receiverId">The receiver ID to filter by (optional).</param>
    /// <param name="billingNumber">The billing number to filter by (optional).</param>
    /// <param name="postalCode">The postal code to filter by (optional).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of return locations.</returns>
    Task<List<ReturnLocation>> GetReturnLocationsAsync(
        string countryCode,
        string? receiverId = null,
        string? billingNumber = null,
        string? postalCode = null,
        CancellationToken cancellationToken = default);
}
