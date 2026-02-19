using ParcelNET.Dhl.LocationFinder.Models;

namespace ParcelNET.Dhl.LocationFinder;

/// <summary>
/// Client interface for the DHL Location Finder Unified API v1.
/// </summary>
public interface IDhlLocationFinderClient
{
    /// <summary>
    /// Finds DHL locations (Packstations, post offices, parcel shops) by address.
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
    /// <param name="city">The city name.</param>
    /// <param name="postalCode">The postal code (optional).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The search result with matching locations.</returns>
    Task<LocationSearchResult> FindByAddressAsync(string countryCode, string city, string? postalCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds DHL locations near a geographic coordinate.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="radiusInMeters">The search radius in meters (optional, default determined by API).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The search result with matching locations.</returns>
    Task<LocationSearchResult> FindByGeoAsync(double latitude, double longitude, int? radiusInMeters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific DHL location by its ID.
    /// </summary>
    /// <param name="locationId">The DHL location ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The location details.</returns>
    Task<DhlLocation> GetLocationByIdAsync(string locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds DHL locations by keyword ID.
    /// </summary>
    /// <param name="keywordId">The keyword ID to search for.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The search result with matching locations.</returns>
    Task<LocationSearchResult> FindByKeywordAsync(string keywordId, CancellationToken cancellationToken = default);
}
