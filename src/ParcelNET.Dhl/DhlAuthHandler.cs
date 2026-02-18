using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl;

/// <summary>
/// HTTP message handler that adds DHL API key and OAuth Bearer token headers for shipping requests.
/// </summary>
public class DhlAuthHandler : DelegatingHandler
{
    private readonly DhlOptions _options;
    private readonly IDhlTokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlAuthHandler"/>.
    /// </summary>
    /// <param name="options">DHL configuration options.</param>
    /// <param name="tokenService">Service for obtaining OAuth tokens.</param>
    public DhlAuthHandler(IOptions<DhlOptions> options, IDhlTokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenService);

        _options = options.Value;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("dhl-api-key", _options.ApiKey);

        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
