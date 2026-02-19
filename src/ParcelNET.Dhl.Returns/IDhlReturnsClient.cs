using ParcelNET.Dhl.Returns.Models;

namespace ParcelNET.Dhl.Returns;

/// <summary>
/// Client interface for the DHL Parcel DE Returns API v1.
/// </summary>
public interface IDhlReturnsClient
{
    /// <summary>
    /// Creates a new return order and generates a return label.
    /// </summary>
    /// <param name="request">The return order request details.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The return order response with shipment number and label.</returns>
    Task<ReturnOrderResponse> CreateReturnOrderAsync(ReturnOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves available return locations (drop-off points) for a given country.
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-3 country code.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of return locations.</returns>
    Task<List<ReturnLocation>> GetReturnLocationsAsync(string countryCode, CancellationToken cancellationToken = default);
}
