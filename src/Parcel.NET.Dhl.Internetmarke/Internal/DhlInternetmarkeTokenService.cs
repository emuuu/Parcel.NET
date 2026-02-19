using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Parcel.NET.Dhl.Internetmarke.Internal;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// Manages Internetmarke OAuth token acquisition and caching.
/// Uses the Internetmarke-specific POST /user endpoint with client_credentials grant type.
/// Registered as a singleton.
/// </summary>
internal sealed class DhlInternetmarkeTokenService : IDisposable
{
    private readonly DhlOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _accessToken;
    private DateTimeOffset _tokenExpiry;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal const string TokenHttpClientName = "Parcel.NET.Dhl.Internetmarke.Token";
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(1);

    public DhlInternetmarkeTokenService(IOptions<DhlOptions> options, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock
            if (_accessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            {
                return _accessToken;
            }

            var username = _options.InternetmarkeUsername ?? _options.Username
                ?? throw new InvalidOperationException("DHL InternetmarkeUsername (or Username) is required for Internetmarke authentication.");
            var password = _options.InternetmarkePassword ?? _options.Password
                ?? throw new InvalidOperationException("DHL InternetmarkePassword (or Password) is required for Internetmarke authentication.");

            using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["username"] = username,
                ["password"] = password,
                ["client_id"] = _options.ApiKey,
                ["client_secret"] = _options.ApiSecret ?? throw new InvalidOperationException("DHL ApiSecret is required for Internetmarke authentication.")
            });

            var httpClient = _httpClientFactory.CreateClient(TokenHttpClientName);
            var tokenUrl = _options.InternetmarkeBaseUrl.TrimEnd('/') + "/user";

            using var response = await httpClient.PostAsync(tokenUrl, requestContent, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new HttpRequestException(
                    $"DHL Internetmarke token request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}",
                    inner: null,
                    statusCode: response.StatusCode);
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync(
                DhlInternetmarkeJsonContext.Default.DhlImTokenResponse, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Failed to deserialize Internetmarke token response.");

            if (tokenResponse.ExpiresIn <= 0)
                throw new InvalidOperationException($"Internetmarke token response contained invalid ExpiresIn: {tokenResponse.ExpiresIn}");

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - TokenRefreshBuffer;

            return _accessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
