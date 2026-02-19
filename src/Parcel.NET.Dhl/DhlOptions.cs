namespace Parcel.NET.Dhl;

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
    /// Gets or sets the Parcel DE Tracking XML API appname credential.
    /// Required for <c>Dhl.Tracking</c> (Parcel DE Tracking v0). Sandbox default: <c>zt12345</c>.
    /// </summary>
    public string? TrackingUsername { get; set; }

    /// <summary>
    /// Gets or sets the Parcel DE Tracking XML API password credential.
    /// Required for <c>Dhl.Tracking</c> (Parcel DE Tracking v0). Sandbox default: <c>geheim</c>.
    /// </summary>
    public string? TrackingPassword { get; set; }

    /// <summary>
    /// Gets or sets separate Internetmarke (Portokasse) username.
    /// If null, <see cref="Username"/> is used instead.
    /// </summary>
    public string? InternetmarkeUsername { get; set; }

    /// <summary>
    /// Gets or sets separate Internetmarke (Portokasse) password.
    /// If null, <see cref="Password"/> is used instead.
    /// </summary>
    public string? InternetmarkePassword { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the DHL sandbox environment.
    /// </summary>
    public bool UseSandbox { get; set; }

    // --- Custom URL overrides ---

    /// <summary>
    /// Gets or sets a custom base URL override for the shipping API.
    /// When set, this takes precedence over the <see cref="UseSandbox"/> setting.
    /// </summary>
    public string? CustomShippingBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Parcel DE Tracking API.
    /// </summary>
    public string? CustomTrackingBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Unified Tracking API.
    /// </summary>
    public string? CustomUnifiedTrackingBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Pickup API.
    /// </summary>
    public string? CustomPickupBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Returns API.
    /// </summary>
    public string? CustomReturnsBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Internetmarke API.
    /// </summary>
    public string? CustomInternetmarkeBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override for the Location Finder API.
    /// </summary>
    public string? CustomLocationFinderBaseUrl { get; set; }

    // --- Computed base URLs ---

    /// <summary>
    /// Gets the effective shipping API base URL (Parcel DE Shipping v2).
    /// </summary>
    public string ShippingBaseUrl => CustomShippingBaseUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/shipping/v2/"
            : "https://api-eu.dhl.com/parcel/de/shipping/v2/");

    /// <summary>
    /// Gets the effective Parcel DE Tracking base URL (v0, XML).
    /// </summary>
    public string TrackingBaseUrl => CustomTrackingBaseUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments"
            : "https://api-eu.dhl.com/parcel/de/tracking/v0/shipments");

    /// <summary>
    /// Gets the effective Unified Tracking base URL (JSON). No sandbox available.
    /// </summary>
    public string UnifiedTrackingBaseUrl => CustomUnifiedTrackingBaseUrl
        ?? "https://api-eu.dhl.com/track/shipments";

    /// <summary>
    /// Gets the effective Pickup API base URL (Parcel DE Pickup v3).
    /// </summary>
    public string PickupBaseUrl => CustomPickupBaseUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/"
            : "https://api-eu.dhl.com/parcel/de/transportation/pickup/v3/");

    /// <summary>
    /// Gets the effective Returns API base URL (Parcel DE Returns v1).
    /// </summary>
    public string ReturnsBaseUrl => CustomReturnsBaseUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/shipping/returns/v1/"
            : "https://api-eu.dhl.com/parcel/de/shipping/returns/v1/");

    /// <summary>
    /// Gets the effective Internetmarke API base URL (Post DE Internetmarke v1). No sandbox available.
    /// </summary>
    public string InternetmarkeBaseUrl => CustomInternetmarkeBaseUrl
        ?? "https://api-eu.dhl.com/post/de/shipping/im/v1/";

    /// <summary>
    /// Gets the effective Location Finder API base URL (Unified v1). No sandbox available.
    /// </summary>
    public string LocationFinderBaseUrl => CustomLocationFinderBaseUrl
        ?? "https://api.dhl.com/location-finder/v1/";

    /// <summary>
    /// Gets or sets a custom token endpoint URL override. When set, this takes precedence over the default DHL ROPC token URL.
    /// </summary>
    public string? CustomTokenUrl { get; set; }

    /// <summary>
    /// Gets the effective OAuth token endpoint URL. Uses sandbox endpoint when <see cref="UseSandbox"/> is true.
    /// </summary>
    internal string TokenUrl => CustomTokenUrl
        ?? (UseSandbox
            ? "https://api-sandbox.dhl.com/parcel/de/account/auth/ropc/v1/token"
            : "https://api-eu.dhl.com/parcel/de/account/auth/ropc/v1/token");
}
