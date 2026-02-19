using System.Net.Http.Headers;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// HTTP message handler that adds OAuth Bearer token for Internetmarke API requests.
/// Uses the Internetmarke-specific POST /user token endpoint (separate from DHL shipping ROPC).
/// </summary>
internal class DhlInternetmarkeAuthHandler : DelegatingHandler
{
    private readonly DhlInternetmarkeTokenService _tokenService;

    public DhlInternetmarkeAuthHandler(DhlInternetmarkeTokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(tokenService);
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
