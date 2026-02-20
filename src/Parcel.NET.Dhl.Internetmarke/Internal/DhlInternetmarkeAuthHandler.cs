using System.Net.Http.Headers;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// HTTP message handler that adds OAuth Bearer token for Internetmarke API requests.
/// Uses the Internetmarke-specific POST /user token endpoint (separate from DHL shipping ROPC).
/// </summary>
public class DhlInternetmarkeAuthHandler : DelegatingHandler
{
    private readonly DhlInternetmarkeTokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlInternetmarkeAuthHandler"/>.
    /// </summary>
    /// <param name="tokenService">The token service for acquiring Internetmarke OAuth tokens.</param>
    public DhlInternetmarkeAuthHandler(DhlInternetmarkeTokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(tokenService);
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
