using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl;

/// <summary>
/// HTTP message handler that adds only the DHL API key header. Used for endpoints that do not require OAuth (e.g. tracking).
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
