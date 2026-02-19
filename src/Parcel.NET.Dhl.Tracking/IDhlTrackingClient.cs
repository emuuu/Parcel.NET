using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Tracking.Models;

namespace Parcel.NET.Dhl.Tracking;

/// <summary>
/// DHL Parcel DE Tracking client interface (XML API v0) with DHL-specific operations.
/// </summary>
public interface IDhlTrackingClient : ITrackingService
{
    /// <summary>
    /// Tracks a shipment using the <c>d-get-piece-detail</c> request (business customer detail tracking).
    /// </summary>
    /// <param name="trackingNumber">The tracking number (piece code).</param>
    /// <param name="options">Optional tracking parameters (language, zip code).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tracking result.</returns>
    Task<TrackingResult> TrackAsync(
        string trackingNumber,
        DhlTrackingOptions? options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracks a shipment using the <c>get-status-for-public-user</c> request (public tracking, no business credentials required).
    /// </summary>
    /// <param name="trackingNumber">The tracking number (piece code).</param>
    /// <param name="options">Optional tracking parameters (language, zip code).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tracking result.</returns>
    Task<TrackingResult> TrackPublicAsync(
        string trackingNumber,
        DhlTrackingOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the proof-of-delivery signature image (GIF) for a shipment.
    /// </summary>
    /// <param name="trackingNumber">The tracking number (piece code).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The signature image as a byte array (GIF format), or null if not available.</returns>
    Task<byte[]?> GetSignatureAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default);
}
