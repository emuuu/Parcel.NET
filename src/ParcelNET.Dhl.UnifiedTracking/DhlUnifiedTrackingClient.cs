using System.Net.Http.Json;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Internal;
using ParcelNET.Dhl.UnifiedTracking.Internal;
using ParcelNET.Dhl.UnifiedTracking.Models;

namespace ParcelNET.Dhl.UnifiedTracking;

/// <summary>
/// DHL Unified Shipment Tracking API client (JSON) implementing <see cref="IDhlUnifiedTrackingClient"/>.
/// </summary>
public class DhlUnifiedTrackingClient : IDhlUnifiedTrackingClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlUnifiedTrackingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Unified Tracking API requests.</param>
    public DhlUnifiedTrackingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public Task<TrackingResult> TrackAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
        => TrackAsync(trackingNumber, null, cancellationToken);

    /// <inheritdoc />
    public async Task<TrackingResult> TrackAsync(
        string trackingNumber,
        DhlUnifiedTrackingOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        var url = BuildTrackingUrl(trackingNumber, options);
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new TrackingException(
                $"DHL Unified Tracking API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var trackingResponse = await response.Content.ReadFromJsonAsync(DhlUnifiedTrackingJsonContext.Default.DhlUnifiedTrackingResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new TrackingException("Failed to deserialize DHL Unified Tracking response.");

        var shipment = trackingResponse.Shipments?.FirstOrDefault()
            ?? throw new TrackingException("DHL Unified Tracking response contained no shipments.");

        return MapToTrackingResult(shipment);
    }

    private static string BuildTrackingUrl(string trackingNumber, DhlUnifiedTrackingOptions? options)
    {
        var url = $"?trackingNumber={Uri.EscapeDataString(trackingNumber)}";

        if (options?.Language is not null)
            url += $"&language={Uri.EscapeDataString(options.Language)}";

        if (options?.RecipientPostalCode is not null)
            url += $"&recipientPostalCode={Uri.EscapeDataString(options.RecipientPostalCode)}";

        if (options?.OriginCountryCode is not null)
            url += $"&originCountryCode={Uri.EscapeDataString(options.OriginCountryCode)}";

        return url;
    }

    private static TrackingResult MapToTrackingResult(DhlUnifiedTrackedShipment shipment) =>
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
                    Timestamp = DateTimeOffset.TryParse(e.Timestamp, out var ts) ? ts : null,
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

    private static string? FormatLocation(DhlUnifiedTrackingLocation? location)
    {
        if (location?.Address is null)
            return null;

        var parts = new[] { location.Address.AddressLocality, location.Address.CountryCode }
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return string.Join(", ", parts);
    }
}
