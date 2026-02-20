namespace Parcel.NET.Dhl.UnifiedTracking.Models;

/// <summary>
/// Options for DHL Unified Tracking requests.
/// </summary>
public class DhlUnifiedTrackingOptions
{
    /// <summary>
    /// Gets the preferred language for tracking event descriptions (e.g. "en", "de").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the recipient postal code for enhanced tracking data.
    /// </summary>
    public string? RecipientPostalCode { get; init; }

    /// <summary>
    /// Gets the origin country code for cross-border shipment tracking.
    /// </summary>
    public string? OriginCountryCode { get; init; }

    /// <summary>
    /// Gets the DHL service type to disambiguate tracking numbers
    /// (e.g. "express", "parcel-de", "parcel-nl", "ecommerce", "freight", "sameday", "post-de").
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Gets the requester country code to adjust display options based on requester location.
    /// </summary>
    public string? RequesterCountryCode { get; init; }
}
