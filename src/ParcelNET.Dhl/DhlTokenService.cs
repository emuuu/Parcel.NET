using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl;

/// <summary>
/// Manages DHL OAuth token acquisition and caching. Registered as a singleton.
/// </summary>
public sealed class DhlTokenService : IDhlTokenService, IDisposable
{
    private readonly DhlOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _accessToken;
    private DateTimeOffset _tokenExpiry;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal const string TokenHttpClientName = "ParcelNET.Dhl.Token";
    private const string TokenUrl = "https://api-eu.dhl.com/parcel/de/account/auth/ropc/v1/token";
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Initializes a new instance of <see cref="DhlTokenService"/>.
    /// </summary>
    /// <param name="options">DHL configuration options.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    public DhlTokenService(IOptions<DhlOptions> options, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_accessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            {
                return _accessToken;
            }

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = _options.Username ?? throw new InvalidOperationException("DHL Username is required for OAuth authentication."),
                ["password"] = _options.Password ?? throw new InvalidOperationException("DHL Password is required for OAuth authentication."),
                ["client_id"] = _options.ApiKey,
                ["client_secret"] = _options.ApiSecret ?? throw new InvalidOperationException("DHL ApiSecret is required for OAuth authentication.")
            });

            var httpClient = _httpClientFactory.CreateClient(TokenHttpClientName);
            var response = await httpClient.PostAsync(TokenUrl, requestContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize DHL token response.");

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - TokenRefreshBuffer;

            return _accessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Disposes the semaphore used for thread synchronization.
    /// </summary>
    public void Dispose()
    {
        _semaphore.Dispose();
    }

    internal sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
