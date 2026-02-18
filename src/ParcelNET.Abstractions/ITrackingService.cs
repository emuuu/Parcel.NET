using ParcelNET.Abstractions.Models;

namespace ParcelNET.Abstractions;

/// <summary>
/// Carrier-agnostic interface for tracking shipments.
/// </summary>
public interface ITrackingService
{
    /// <summary>
    /// Tracks a shipment by its tracking number.
    /// </summary>
    /// <param name="trackingNumber">The tracking number to look up.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tracking result with current status and event history.</returns>
    /// <exception cref="Exceptions.TrackingException">Thrown when the carrier API returns an error.</exception>
    Task<TrackingResult> TrackAsync(string trackingNumber, CancellationToken cancellationToken = default);
}
