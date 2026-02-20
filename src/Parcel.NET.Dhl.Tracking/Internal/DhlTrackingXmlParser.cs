using System.Xml.Linq;
using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Dhl.Tracking.Internal;

/// <summary>
/// Parses Parcel DE Tracking v0 XML responses into <see cref="TrackingResult"/>.
/// The DHL API returns all elements as &lt;data&gt; tags differentiated by their <c>name</c> attribute.
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

        // Navigate: root <data name="piece-shipment-list"> -> <data name="piece-shipment"> (or direct child <data>)
        var shipmentData = FindChildByName(data, "piece-shipment")
            ?? data.Element("data")
            ?? data;

        var pieceCode = shipmentData.Attribute("piece-code")?.Value
            ?? shipmentData.Attribute("searched-piece-code")?.Value
            ?? "";

        var deliveryEventFlag = shipmentData.Attribute("delivery-event-flag")?.Value;
        var pieceStatus = shipmentData.Attribute("piece-status")?.Value;

        var events = ParseEvents(shipmentData);
        var estimatedDelivery = ParseEstimatedDelivery(shipmentData);

        return new TrackingResult
        {
            ShipmentNumber = pieceCode,
            Status = MapDeliveryStatus(deliveryEventFlag, pieceStatus),
            EstimatedDelivery = estimatedDelivery,
            Events = events
        };
    }

    /// <summary>
    /// Parses the <c>d-get-signature</c> response and returns the hex-encoded signature image string.
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

    private static List<TrackingEvent> ParseEvents(XElement shipmentData)
    {
        var events = new List<TrackingEvent>();

        // DHL returns events as <data name="piece-event-list"> containing <data name="piece-event"> children.
        // Some responses may also use direct element names (e.g. <piece-event-list>), so we check both.
        var eventList = FindChildByName(shipmentData, "piece-event-list")
            ?? shipmentData.Element("piece-event-list");

        if (eventList is not null)
        {
            var eventElements = FindChildrenByName(eventList, "piece-event");
            if (!eventElements.Any())
                eventElements = eventList.Elements("piece-event");

            foreach (var ev in eventElements)
            {
                events.Add(ParseSingleEvent(ev));
            }
        }

        // get-status-for-public-user returns piece-status-public-list
        var publicList = FindChildByName(shipmentData, "piece-status-public-list")
            ?? shipmentData.Element("piece-status-public-list");

        if (publicList is not null)
        {
            var publicEvents = FindChildrenByName(publicList, "piece-status-public");
            if (!publicEvents.Any())
                publicEvents = publicList.Elements("piece-status-public");

            foreach (var ev in publicEvents)
            {
                events.Add(ParseSingleEvent(ev));
            }
        }

        return events;
    }

    private static DateTimeOffset? ParseEstimatedDelivery(XElement shipmentData)
    {
        var deliveryDate = shipmentData.Attribute("delivery-date")?.Value;
        if (deliveryDate is not null && DateTimeOffset.TryParse(deliveryDate, out var dt))
            return dt;

        if (deliveryDate is not null && TryParseGermanTimestamp(deliveryDate) is { } parsed)
            return parsed;

        return null;
    }

    /// <summary>
    /// Finds a child &lt;data&gt; element whose <c>name</c> attribute matches the given value.
    /// </summary>
    private static XElement? FindChildByName(XElement parent, string nameValue)
        => parent.Elements("data").FirstOrDefault(e => e.Attribute("name")?.Value == nameValue);

    /// <summary>
    /// Finds all child &lt;data&gt; elements whose <c>name</c> attribute matches the given value.
    /// </summary>
    private static IEnumerable<XElement> FindChildrenByName(XElement parent, string nameValue)
        => parent.Elements("data").Where(e => e.Attribute("name")?.Value == nameValue);

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
            "0" => TrackingStatus.PreTransit,        // Announced
            "1" => TrackingStatus.PreTransit,         // Picked up
            "2" => TrackingStatus.InTransit,          // In transit
            "3" => TrackingStatus.InTransit,          // Sorting center
            "4" => TrackingStatus.OutForDelivery,     // Out for delivery
            "5" => TrackingStatus.Delivered,           // Delivered
            "6" => TrackingStatus.Exception,           // Exception
            "7" => TrackingStatus.Returned,            // Returned
            _ => TrackingStatus.Unknown
        };
    }
}
