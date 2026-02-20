using System.Globalization;
using System.Net.Http.Json;
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.GoExpress.Internal;
using Parcel.NET.GoExpress.Tracking.Internal;

namespace Parcel.NET.GoExpress.Tracking;

/// <summary>
/// GO! Express Tracking API client implementing <see cref="ITrackingService"/>.
/// </summary>
public class GoExpressTrackingClient : ITrackingService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="GoExpressTrackingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for GO! Express Tracking API requests.</param>
    public GoExpressTrackingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<TrackingResult> TrackAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        using var response = await _httpClient.GetAsync(
            $"status?language=de&hwbNumber={Uri.EscapeDataString(trackingNumber)}",
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = GoExpressErrorHelper.TryParseErrorDetail(rawBody);
            throw new TrackingException(
                $"GO! Express Tracking API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var trackingResponse = await response.Content.ReadFromJsonAsync(
            GoExpressTrackingJsonContext.Default.GoExpressTrackingResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new TrackingException("Failed to deserialize GO! Express tracking response.");

        return MapToTrackingResult(trackingNumber, trackingResponse);
    }

    private static TrackingResult MapToTrackingResult(string trackingNumber, GoExpressTrackingResponse response)
    {
        var items = response.TrackingItems;
        var events = new List<TrackingEvent>();

        if (items?.TrackingTable is not null)
        {
            foreach (var entry in items.TrackingTable)
            {
                DateTimeOffset? timestamp = null;
                if (entry.StatusDate is not null &&
                    DateTimeOffset.TryParse(entry.StatusDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    timestamp = parsed;
                }

                events.Add(new TrackingEvent
                {
                    Timestamp = timestamp,
                    Location = entry.Station,
                    Description = entry.Status ?? "",
                    StatusCode = entry.StatusCode
                });
            }
        }

        var status = MapTransportStatus(items?.TransportStatus);

        return new TrackingResult
        {
            ShipmentNumber = trackingNumber,
            Status = status,
            Events = events
        };
    }

    private static TrackingStatus MapTransportStatus(string? transportStatus) => transportStatus switch
    {
        "GO10" => TrackingStatus.PreTransit,
        "GO20" => TrackingStatus.InTransit,
        "GO40" => TrackingStatus.InTransit,
        "GO42" => TrackingStatus.OutForDelivery,
        "GO50" => TrackingStatus.Delivered,
        "GO90" => TrackingStatus.Returned,
        _ => TrackingStatus.Unknown
    };
}
