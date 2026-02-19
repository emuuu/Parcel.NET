namespace ParcelNET.Dhl.Tracking.Models;

/// <summary>
/// DHL-specific options for Parcel DE Tracking v0 (XML) requests.
/// </summary>
public class DhlTrackingOptions
{
    /// <summary>
    /// Gets the preferred language code for tracking event descriptions (e.g. "de", "en"). Defaults to "de".
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the recipient zip code for enhanced tracking data (required for some request types).
    /// </summary>
    public string? ZipCode { get; init; }
}
