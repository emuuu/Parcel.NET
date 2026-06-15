using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl.EPost.Internal;

/// <summary>
/// Manages E-POSTBUSINESS API JWT acquisition and caching via <c>POST /api/Login</c>.
/// The login takes a JSON body (vendorID/EKP/secret/password) and returns a bearer token.
/// The API does not return an expiry, so the lifetime is derived from the requested
/// <see cref="DhlOptions.EPostTokenDurationMinutes"/> (default/maximum 1440 minutes).
/// Registered as a singleton.
/// </summary>
public sealed class DhlEPostTokenService : IDisposable
{
    private readonly DhlOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _token;
    private DateTimeOffset _tokenExpiry;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal const string TokenHttpClientName = "Parcel.NET.Dhl.EPost.Token";
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(1);
    private const int DefaultTokenDurationMinutes = 1440;

    /// <summary>Initializes a new instance of <see cref="DhlEPostTokenService"/>.</summary>
    /// <param name="options">DHL configuration options.</param>
    /// <param name="httpClientFactory">HTTP client factory for token requests.</param>
    public DhlEPostTokenService(IOptions<DhlOptions> options, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>Gets a cached or freshly acquired JWT for E-POSTBUSINESS API requests.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A valid bearer token string.</returns>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_token is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            return _token;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_token is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            {
                return _token;
            }

            var durationMinutes = _options.EPostTokenDurationMinutes ?? DefaultTokenDurationMinutes;

            var loginRequest = new DhlEPostLoginRequest
            {
                VendorID = _options.EPostVendorId
                    ?? throw new InvalidOperationException("DHL EPostVendorId is required for E-POST authentication."),
                Ekp = _options.EPostEkp
                    ?? throw new InvalidOperationException("DHL EPostEkp is required for E-POST authentication."),
                Secret = _options.EPostSecret
                    ?? throw new InvalidOperationException("DHL EPostSecret is required for E-POST authentication."),
                Password = _options.EPostPassword
                    ?? throw new InvalidOperationException("DHL EPostPassword is required for E-POST authentication."),
                VendorSubID = _options.EPostVendorSubId,
                TokenDuration = durationMinutes
            };

            var httpClient = _httpClientFactory.CreateClient(TokenHttpClientName);
            httpClient.BaseAddress ??= new Uri(_options.EPostBaseUrl);

            using var response = await httpClient.PostAsJsonAsync(
                "api/Login",
                loginRequest,
                DhlEPostJsonContext.Default.DhlEPostLoginRequest,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var apiError = DhlEPostErrorParser.TryParse(rawBody);
                throw new Parcel.NET.Abstractions.Exceptions.ParcelException(
                    apiError is not null
                        ? $"E-POST login returned {(int)response.StatusCode}: [{apiError.Code}] {apiError.Description}"
                        : $"E-POST login returned {(int)response.StatusCode}.",
                    response.StatusCode,
                    apiError?.Code ?? ((int)response.StatusCode).ToString(),
                    rawBody);
            }

            var loginResponse = await response.Content.ReadFromJsonAsync(
                DhlEPostJsonContext.Default.DhlEPostLoginResponse,
                cancellationToken).ConfigureAwait(false);

            if (loginResponse is null || string.IsNullOrEmpty(loginResponse.Token))
            {
                throw new Parcel.NET.Abstractions.Exceptions.ParcelException("E-POST login returned an empty token.");
            }

            _token = loginResponse.Token;
            _tokenExpiry = DateTimeOffset.UtcNow.AddMinutes(durationMinutes) - TokenRefreshBuffer;
            return _token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Discards the cached token so the next <see cref="GetTokenAsync"/> performs a fresh login.
    /// Used by the auth handler when the API rejects a request with HTTP 401 (token expired server-side).
    /// </summary>
    public void Invalidate()
    {
        _token = null;
        _tokenExpiry = default;
    }

    /// <inheritdoc />
    public void Dispose() => _semaphore.Dispose();
}
