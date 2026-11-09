using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl;

/// <summary>
/// HTTP message handler that adds the <c>dhl-api-key</c> header. Used for endpoints that do not require OAuth
/// (Unified Tracking, Location Finder) and for Parcel DE Tracking v0, which additionally needs Basic authentication
/// with the API key and secret; <c>DhlTrackingClient</c> sets that header itself.
/// </summary>
public class DhlApiKeyHandler : DelegatingHandler
{
    private readonly DhlOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlApiKeyHandler"/>.
    /// </summary>
    /// <param name="options">DHL configuration options.</param>
    public DhlApiKeyHandler(IOptions<DhlOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("dhl-api-key", _options.ApiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
