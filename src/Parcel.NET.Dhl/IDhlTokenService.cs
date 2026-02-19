namespace Parcel.NET.Dhl;

/// <summary>
/// Service responsible for obtaining and caching DHL OAuth tokens.
/// </summary>
public interface IDhlTokenService
{
    /// <summary>
    /// Gets a valid OAuth access token, refreshing the cached token if necessary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A valid access token string.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
