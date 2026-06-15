using System.Net;
using System.Net.Http.Headers;

namespace Parcel.NET.Dhl.EPost.Internal;

/// <summary>
/// HTTP message handler that adds the E-POSTBUSINESS API JWT as a <c>Bearer</c> token.
/// All routes require it except Login, SetPassword, smsRequest and HealthCheck.
/// On an HTTP 401 the cached token is discarded and the request is retried once with a fresh login,
/// because the API returns no token expiry and the cached lifetime is only an estimate.
/// </summary>
public class DhlEPostAuthHandler : DelegatingHandler
{
    private readonly DhlEPostTokenService _tokenService;

    /// <summary>Initializes a new instance of <see cref="DhlEPostAuthHandler"/>.</summary>
    /// <param name="tokenService">The token service for acquiring E-POST JWTs.</param>
    public DhlEPostAuthHandler(DhlEPostTokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(tokenService);
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // The Login endpoints (Login, setPassword, smsRequest, HealthCheck) require no token and must stay
        // callable without one — e.g. HealthCheck has to work even when credentials are missing or expired.
        if (IsAuthExempt(request))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // Buffer the body so the request can be replayed if a token refresh is needed.
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        }

        var token = await _tokenService.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // Token likely expired server-side: drop it, acquire a fresh one and retry exactly once.
        // Dispose the 401 response even if acquiring the fresh token or cloning the request throws.
        HttpRequestMessage retry;
        string freshToken;
        try
        {
            _tokenService.Invalidate();
            freshToken = await _tokenService.GetTokenAsync(cancellationToken).ConfigureAwait(false);
            retry = await CloneRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            response.Dispose();
        }

        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", freshToken);
        return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
    }

    // Endpoints under /api/Login (Login, setPassword, smsRequest, HealthCheck) are exempt from the JWT.
    private static bool IsAuthExempt(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath;
        return path is not null && path.Contains("/api/Login", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
