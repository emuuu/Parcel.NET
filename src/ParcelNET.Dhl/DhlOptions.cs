namespace ParcelNET.Dhl;

/// <summary>
/// Configuration options for DHL API integration.
/// </summary>
public class DhlOptions
{
    /// <summary>
    /// Gets or sets the DHL API key (also used as OAuth client_id).
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the DHL API secret (OAuth client_secret). Required for shipping operations.
    /// </summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// Gets or sets the DHL ROPC username. Required for shipping operations.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the DHL ROPC password. Required for shipping operations.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the DHL sandbox environment.
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the shipping API.
    /// When set, this takes precedence over the <see cref="UseSandbox"/> setting.
    /// </summary>
    public string? CustomShippingBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the tracking API.
    /// When set, this takes precedence over the default tracking URL.
    /// </summary>
    public string? CustomTrackingBaseUrl { get; set; }

    /// <summary>
    /// Gets the effective shipping API base URL. Uses <see cref="CustomShippingBaseUrl"/> if set,
    /// otherwise selects sandbox or production based on <see cref="UseSandbox"/>.
    /// </summary>
    public string ShippingBaseUrl => CustomShippingBaseUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/shipping/v2"
            : "https://api-eu.dhl.com/parcel/de/shipping/v2");

    /// <summary>
    /// Gets the effective tracking API base URL. Uses <see cref="CustomTrackingBaseUrl"/> if set,
    /// otherwise returns the default production URL.
    /// </summary>
    public string TrackingBaseUrl => CustomTrackingBaseUrl
        ?? "https://api-eu.dhl.com/track/shipments";

    /// <summary>
    /// Gets or sets a custom token endpoint URL override. When set, this takes precedence over the default DHL ROPC token URL.
    /// </summary>
    public string? CustomTokenUrl { get; set; }

    /// <summary>
    /// Gets the effective OAuth token endpoint URL.
    /// </summary>
    internal string TokenUrl => CustomTokenUrl
        ?? "https://api-eu.dhl.com/parcel/de/account/auth/ropc/v1/token";
}
