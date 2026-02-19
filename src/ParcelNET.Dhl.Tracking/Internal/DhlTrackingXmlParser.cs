using System.Xml.Linq;
using ParcelNET.Abstractions.Models;

namespace ParcelNET.Dhl.Tracking.Internal;

/// <summary>
/// Parses Parcel DE Tracking v0 XML responses into <see cref="TrackingResult"/>.
/// </summary>
internal static class DhlTrackingXmlParser
{
    /// <summary>
    /// Parses the XML response from <c>d-get-piece-detail</c> or <c>get-status-for-public-user</c>.
    /// </summary>
    internal static TrackingResult ParseTrackingResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var data = doc.Root
            ?? throw new InvalidOperationException("XML response has no root element.");

        // The response has: <data ...><data ...> with piece-status-list and piece-event-list
        var innerData = data.Element("data") ?? data;

        var pieceCode = innerData.Attribute("piece-code")?.Value
            ?? innerData.Attribute("searched-piece-code")?.Value
            ?? "";

        var deliveryEventFlag = innerData.Attribute("delivery-event-flag")?.Value;
        var pieceStatus = innerData.Attribute("piece-status")?.Value;

        var events = ParseEvents(innerData);

        return new TrackingResult
        {
            ShipmentNumber = pieceCode,
            Status = MapDeliveryStatus(deliveryEventFlag, pieceStatus),
            EstimatedDelivery = null,
            Events = events
        };
    }

    /// <summary>
    /// Parses the <c>d-get-signature</c> response and returns the Base64-encoded signature image.
    /// </summary>
    internal static string? ParseSignatureResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var data = doc.Root;
        var innerData = data?.Element("data") ?? data;
        return innerData?.Attribute("image")?.Value;
    }

    /// <summary>
    /// Extracts the error code from a response. Returns 0 if the request was successful.
    /// </summary>
    internal static int GetErrorCode(string xml)
    {
        var doc = XDocument.Parse(xml);
        var data = doc.Root;
        var innerData = data?.Element("data") ?? data;
        var code = innerData?.Attribute("code")?.Value ?? data?.Attribute("code")?.Value;
        return int.TryParse(code, out var c) ? c : -1;
    }

    /// <summary>
    /// Extracts the error message from a response.
    /// </summary>
    internal static string? GetErrorMessage(string xml)
    {
        var doc = XDocument.Parse(xml);
        var data = doc.Root;
        var innerData = data?.Element("data") ?? data;
        return innerData?.Attribute("error")?.Value
            ?? innerData?.Attribute("error-status")?.Value
            ?? data?.Attribute("error")?.Value;
    }

    private static List<TrackingEvent> ParseEvents(XElement data)
    {
        var events = new List<TrackingEvent>();

        // d-get-piece-detail returns piece-event-list with piece-event children
        var eventList = data.Element("piece-event-list");
        if (eventList is not null)
        {
            foreach (var ev in eventList.Elements("piece-event"))
            {
                events.Add(ParseSingleEvent(ev));
            }
        }

        // get-status-for-public-user returns piece-status-public-list
        var publicList = data.Element("piece-status-public-list");
        if (publicList is not null)
        {
            foreach (var ev in publicList.Elements("piece-status-public"))
            {
                events.Add(ParseSingleEvent(ev));
            }
        }

        return events;
    }

    private static TrackingEvent ParseSingleEvent(XElement ev)
    {
        var eventTimestamp = ev.Attribute("event-timestamp")?.Value;
        var eventLocation = ev.Attribute("event-location")?.Value;
        var eventCountry = ev.Attribute("event-country")?.Value;
        var eventStatus = ev.Attribute("event-status")?.Value;
        var eventText = ev.Attribute("event-text")?.Value;
        var eventShortStatus = ev.Attribute("event-short-status")?.Value;
        var standardEventCode = ev.Attribute("standard-event-code")?.Value;

        var locationParts = new[] { eventLocation, eventCountry }
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return new TrackingEvent
        {
            Timestamp = eventTimestamp is not null
                ? DateTimeOffset.TryParse(eventTimestamp, out var ts) ? ts : TryParseGermanTimestamp(eventTimestamp)
                : null,
            Location = string.Join(", ", locationParts) is { Length: > 0 } loc ? loc : null,
            Description = eventText ?? eventStatus ?? "Unknown",
            StatusCode = standardEventCode ?? eventShortStatus
        };
    }

    private static DateTimeOffset? TryParseGermanTimestamp(string value)
    {
        // DHL sometimes returns timestamps like "18.02.2026 10:00"
        if (DateTimeOffset.TryParseExact(value, "dd.MM.yyyy HH:mm",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeLocal, out var dt))
        {
            return dt;
        }

        return null;
    }

    private static TrackingStatus MapDeliveryStatus(string? deliveryEventFlag, string? pieceStatus)
    {
        // delivery-event-flag: "1" means delivered
        if (deliveryEventFlag == "1")
            return TrackingStatus.Delivered;

        // piece-status maps to various states
        return pieceStatus switch
        {
            "0" => TrackingStatus.PreTransit,      // Announced
            "1" => TrackingStatus.PreTransit,       // Picked up
            "2" => TrackingStatus.InTransit,        // In transit
            "3" => TrackingStatus.InTransit,        // Sorting center
            "4" => TrackingStatus.InTransit,        // Out for delivery
            "5" => TrackingStatus.Delivered,         // Delivered
            "6" => TrackingStatus.Exception,         // Exception
            "7" => TrackingStatus.Returned,          // Returned
            _ => TrackingStatus.Unknown
        };
    }
}
