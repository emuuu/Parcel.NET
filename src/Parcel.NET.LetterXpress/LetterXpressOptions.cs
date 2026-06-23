namespace Parcel.NET.LetterXpress;

/// <summary>
/// Configuration options for the LetterXpress (A&amp;O Fischer) API v3 integration.
/// </summary>
public class LetterXpressOptions
{
    private const string ProductionBaseUrl = "https://api.letterxpress.de/v3/";

    /// <summary>
    /// Gets or sets the LetterXpress account username.
    /// Generated under "Mein Konto &gt; Zugangsdaten &gt; LXP API".
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the LetterXpress API key.
    /// Generated under "Mein Konto &gt; Zugangsdaten &gt; LXP API".
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the LetterXpress test mode.
    /// In test mode (<c>"mode": "test"</c>) submitted jobs are not processed but stored in the
    /// "Postbox" where they can be reviewed, deleted, or sent. Postbox jobs are deleted after 7 days.
    /// When <see langword="false"/>, jobs are processed directly (<c>"mode": "live"</c>).
    /// </summary>
    public bool UseTestMode { get; set; }

    /// <summary>
    /// Gets or sets a custom base URL override.
    /// When set, this takes precedence over the default production URL.
    /// </summary>
    public string? CustomBaseUrl { get; set; }

    /// <summary>
    /// Gets the effective API base URL. Uses <see cref="CustomBaseUrl"/> if set,
    /// otherwise the production URL. A trailing slash is appended if missing so that
    /// relative request URIs (e.g. <c>balance</c>, <c>printjobs</c>) resolve correctly.
    /// </summary>
    public string BaseUrl => EnsureTrailingSlash(CustomBaseUrl) ?? ProductionBaseUrl;

    private static string? EnsureTrailingSlash(string? url) =>
        url is null ? null : url.EndsWith('/') ? url : url + "/";

    /// <summary>
    /// Gets the <c>mode</c> value sent in the request <c>auth</c> object (<c>test</c> or <c>live</c>).
    /// </summary>
    internal string Mode => UseTestMode ? "test" : "live";
}
