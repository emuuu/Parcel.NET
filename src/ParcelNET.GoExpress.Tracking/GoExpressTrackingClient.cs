using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Models;

namespace ParcelNET.GoExpress.Tracking;

/// <summary>
/// GO! Express Tracking API client. Currently a stub — the GO! Express Tracking API
/// specification is not yet publicly available.
/// </summary>
public class GoExpressTrackingClient : ITrackingService
{
    /// <summary>
    /// Initializes a new instance of <see cref="GoExpressTrackingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for GO! Express Tracking API requests.</param>
    public GoExpressTrackingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
    }

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">
    /// Always thrown. The GO! Express Tracking API specification is not yet publicly available.
    /// </exception>
    public Task<TrackingResult> TrackAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement when GO! Express publishes the Shipment Tracker API specification.
        throw new NotImplementedException(
            "GO! Express Tracking API specification is not yet publicly available. Contact GO! Express for API access.");
    }
}
