using System.Net.Http.Json;
using System.Text.Json;
using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Tracking.Internal;
using ParcelNET.Dhl.Tracking.Models;

namespace ParcelNET.Dhl.Tracking;

/// <summary>
/// DHL Shipment Tracking API v1 client implementing <see cref="ITrackingService"/>.
/// </summary>
public class DhlTrackingClient : ITrackingService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of <see cref="DhlTrackingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Tracking API requests.</param>
    public DhlTrackingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public Task<TrackingResult> TrackAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
        => TrackAsync(trackingNumber, null, cancellationToken);

    /// <summary>
    /// Tracks a shipment with DHL-specific options.
    /// </summary>
    /// <param name="trackingNumber">The tracking number to look up.</param>
    /// <param name="options">Optional DHL-specific tracking parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tracking result.</returns>
    /// <exception cref="TrackingException">Thrown when the DHL API returns an error.</exception>
    public async Task<TrackingResult> TrackAsync(
        string trackingNumber,
        DhlTrackingOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var url = BuildTrackingUrl(trackingNumber, options);
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new TrackingException(
                $"DHL Tracking API returned {(int)response.StatusCode}.",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var trackingResponse = await response.Content.ReadFromJsonAsync<DhlTrackingResponse>(JsonOptions, cancellationToken)
            ?? throw new TrackingException("Failed to deserialize DHL tracking response.");

        var shipment = trackingResponse.Shipments?.FirstOrDefault()
            ?? throw new TrackingException("DHL tracking response contained no shipments.");

        return MapToTrackingResult(shipment);
    }

    private static string BuildTrackingUrl(string trackingNumber, DhlTrackingOptions? options)
    {
        var url = $"?trackingNumber={Uri.EscapeDataString(trackingNumber)}";

        if (options?.Language is not null)
        {
            url += $"&language={Uri.EscapeDataString(options.Language)}";
        }

        if (options?.RecipientPostalCode is not null)
        {
            url += $"&recipientPostalCode={Uri.EscapeDataString(options.RecipientPostalCode)}";
        }

        if (options?.OriginCountryCode is not null)
        {
            url += $"&originCountryCode={Uri.EscapeDataString(options.OriginCountryCode)}";
        }

        return url;
    }

    private static TrackingResult MapToTrackingResult(DhlTrackedShipment shipment) =>
        new()
        {
            ShipmentNumber = shipment.Id,
            Status = MapStatus(shipment.Status?.StatusCode),
            EstimatedDelivery = shipment.EstimatedTimeOfDelivery is not null
                ? DateTimeOffset.TryParse(shipment.EstimatedTimeOfDelivery, out var dt) ? dt : null
                : null,
            Events = shipment.Events?
                .Select(e => new TrackingEvent
                {
                    Timestamp = DateTimeOffset.TryParse(e.Timestamp, out var ts) ? ts : DateTimeOffset.MinValue,
                    Location = FormatLocation(e.Location),
                    Description = e.Description ?? e.Status ?? "Unknown",
                    StatusCode = e.StatusCode
                })
                .ToList() ?? []
        };

    private static TrackingStatus MapStatus(string? statusCode) => statusCode switch
    {
        "pre-transit" => TrackingStatus.PreTransit,
        "transit" => TrackingStatus.InTransit,
        "delivered" => TrackingStatus.Delivered,
        "failure" => TrackingStatus.Exception,
        _ => TrackingStatus.Unknown
    };

    private static string? FormatLocation(DhlTrackingLocation? location)
    {
        if (location?.Address is null)
        {
            return null;
        }

        var parts = new[] { location.Address.AddressLocality, location.Address.CountryCode }
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return string.Join(", ", parts);
    }
}
