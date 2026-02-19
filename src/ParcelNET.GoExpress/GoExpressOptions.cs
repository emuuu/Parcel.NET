namespace ParcelNET.GoExpress;

/// <summary>
/// Configuration options for GO! Express &amp; Logistics API integration.
/// </summary>
public class GoExpressOptions
{
    private const string ProductionBaseUrl = "https://ws.api.general-overnight.com/external/ci/";
    private const string SandboxBaseUrl = "https://ws-tst.api.general-overnight.com/external/ci/";

    /// <summary>
    /// Gets or sets the GO! Connect API username for Basic Auth.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the GO! Connect API password for Basic Auth.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the 3-character responsible station code (e.g. "FRA").
    /// Required for shipping operations.
    /// </summary>
    public string? ResponsibleStation { get; set; }

    /// <summary>
    /// Gets or sets the customer ID (max 7 characters).
    /// </summary>
    public required string CustomerId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the GO! Express sandbox environment.
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override.
    /// When set, this takes precedence over the <see cref="UseSandbox"/> setting.
    /// </summary>
    public string? CustomBaseUrl { get; set; }

    /// <summary>
    /// Gets the effective API base URL. Uses <see cref="CustomBaseUrl"/> if set,
    /// otherwise selects sandbox or production based on <see cref="UseSandbox"/>.
    /// </summary>
    public string BaseUrl => CustomBaseUrl
        ?? (UseSandbox ? SandboxBaseUrl : ProductionBaseUrl);
}
