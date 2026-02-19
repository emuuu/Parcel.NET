using Parcel.NET.Abstractions;
using Parcel.NET.Dhl.UnifiedTracking.Models;

namespace Parcel.NET.Dhl.UnifiedTracking;

/// <summary>
/// DHL Unified Tracking client interface with DHL-specific tracking options.
/// </summary>
public interface IDhlUnifiedTrackingClient : ITrackingService
{
    /// <summary>
    /// Tracks a shipment with DHL-specific options.
    /// </summary>
    /// <param name="trackingNumber">The tracking number to look up.</param>
    /// <param name="options">Optional DHL-specific tracking parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tracking result.</returns>
    Task<Abstractions.Models.TrackingResult> TrackAsync(
        string trackingNumber,
        DhlUnifiedTrackingOptions? options,
        CancellationToken cancellationToken = default);
}
