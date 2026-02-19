using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.LocationFinder.Internal;
using Parcel.NET.Dhl.LocationFinder.Models;

namespace Parcel.NET.Dhl.LocationFinder;

/// <summary>
/// DHL Location Finder Unified API v1 client implementing <see cref="IDhlLocationFinderClient"/>.
/// </summary>
public class DhlLocationFinderClient : IDhlLocationFinderClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlLocationFinderClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Location Finder API requests.</param>
    public DhlLocationFinderClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<LocationSearchResult> FindByAddressAsync(
        string countryCode,
        string city,
        string? postalCode = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var url = $"find-by-address?countryCode={Uri.EscapeDataString(countryCode)}&addressLocality={Uri.EscapeDataString(city)}";
        if (postalCode is not null)
            url += $"&postalCode={Uri.EscapeDataString(postalCode)}";

        return await ExecuteSearchAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<LocationSearchResult> FindByGeoAsync(
        double latitude,
        double longitude,
        int? radiusInMeters = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"find-by-geo?latitude={latitude}&longitude={longitude}";
        if (radiusInMeters.HasValue)
            url += $"&radius={radiusInMeters.Value}";

        return await ExecuteSearchAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DhlLocation> GetLocationByIdAsync(
        string locationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locationId);

        var url = $"locations/{Uri.EscapeDataString(locationId)}";
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new TrackingException(
                $"DHL Location Finder API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlLocationFinderJsonContext.Default.DhlLocationFinderSingleResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new TrackingException("Failed to deserialize DHL Location Finder response.");

        return MapSingleLocation(apiResponse);
    }

    /// <inheritdoc />
    public async Task<LocationSearchResult> FindByKeywordAsync(
        string keywordId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keywordId);

        var url = $"find-by-keyword-id?keywordId={Uri.EscapeDataString(keywordId)}";
        return await ExecuteSearchAsync(url, cancellationToken).ConfigureAwait(false);
    }

    private async Task<LocationSearchResult> ExecuteSearchAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new TrackingException(
                $"DHL Location Finder API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlLocationFinderJsonContext.Default.DhlLocationFinderResponse,
            cancellationToken).ConfigureAwait(false);

        return new LocationSearchResult
        {
            Locations = apiResponse?.Locations?
                .Select(MapLocation)
                .ToList() ?? []
        };
    }

    private static DhlLocation MapLocation(DhlLocationFinderItem item)
    {
        var locationId = item.Location?.Ids?.FirstOrDefault()?.LocationId ?? "";
        var locationType = item.Location?.Type;

        return new DhlLocation
        {
            Id = locationId,
            Name = item.Name ?? "",
            Type = locationType,
            Street = item.Place?.Address?.StreetAddress,
            PostalCode = item.Place?.Address?.PostalCode,
            City = item.Place?.Address?.AddressLocality,
            CountryCode = item.Place?.Address?.CountryCode,
            Latitude = item.Place?.Geo?.Latitude,
            Longitude = item.Place?.Geo?.Longitude,
            DistanceInMeters = item.Distance,
            OpeningHours = FormatOpeningHours(item.OpeningHours),
            Services = item.ServiceTypes ?? []
        };
    }

    private static DhlLocation MapSingleLocation(DhlLocationFinderSingleResponse item)
    {
        var locationId = item.Location?.Ids?.FirstOrDefault()?.LocationId ?? "";
        var locationType = item.Location?.Type;

        return new DhlLocation
        {
            Id = locationId,
            Name = item.Name ?? "",
            Type = locationType,
            Street = item.Place?.Address?.StreetAddress,
            PostalCode = item.Place?.Address?.PostalCode,
            City = item.Place?.Address?.AddressLocality,
            CountryCode = item.Place?.Address?.CountryCode,
            Latitude = item.Place?.Geo?.Latitude,
            Longitude = item.Place?.Geo?.Longitude,
            OpeningHours = FormatOpeningHours(item.OpeningHours),
            Services = item.ServiceTypes ?? []
        };
    }

    private static string? FormatOpeningHours(List<DhlLocationFinderOpeningHours>? hours)
    {
        if (hours is null or { Count: 0 })
            return null;

        return string.Join("; ", hours
            .Where(h => h.DayOfWeek is not null)
            .Select(h => $"{h.DayOfWeek}: {h.Opens}-{h.Closes}"));
    }
}
