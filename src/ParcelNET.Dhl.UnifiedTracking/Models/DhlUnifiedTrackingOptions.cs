namespace ParcelNET.Dhl.UnifiedTracking.Models;

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
}
