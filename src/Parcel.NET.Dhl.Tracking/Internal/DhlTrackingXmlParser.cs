using System.Globalization;
using System.Xml.Linq;
using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Dhl.Tracking.Internal;

/// <summary>
/// Parses Parcel DE Tracking v0 XML responses into <see cref="TrackingResult"/>.
/// The DHL API returns all elements as &lt;data&gt; tags differentiated by their <c>name</c> attribute.
/// </summary>
internal static class DhlTrackingXmlParser
{
    private const string PieceShipment = "piece-shipment";
    private const string PieceEventList = "piece-event-list";
    private const string PieceEvent = "piece-event";
    private const string PieceStatusPublicList = "piece-status-public-list";
    private const string PieceStatusPublic = "piece-status-public";

    /// <summary>
    /// Parses the XML response from <c>d-get-piece-detail</c> or <c>get-status-for-public-user</c>.
    /// Only the first shipment of the response is evaluated.
    /// </summary>
    /// <remarks>
    /// The current state (<see cref="TrackingResult.Status"/>) is taken from the attributes of the shipment element
    /// (<c>piece-shipment</c> or <c>piece-status-public</c>); if it carries neither <c>ice</c> nor
    /// <c>standard-event-code</c>, the last <c>piece-event</c> is used. See <see cref="DhlTrackingStatusMapper"/>.
    /// </remarks>
    internal static TrackingResult ParseTrackingResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root
            ?? throw new InvalidOperationException("XML response has no root element.");

        var shipment = FindShipment(root);

        var pieceCode = Value(shipment, "piece-code")
            ?? Value(shipment, "searched-piece-code")
            ?? "";

        var events = ParseEvents(shipment);

        return new TrackingResult
        {
            ShipmentNumber = pieceCode,
            Status = DetermineStatus(shipment, events.LastSourceElement),
            EstimatedDelivery = DhlTrackingTimestamp.ParseDeliveryDate(Value(shipment, "delivery-date")),
            Events = events.Events
        };
    }

    /// <summary>
    /// Parses the <c>d-get-signature</c> response and returns the hex-encoded signature image string.
    /// </summary>
    internal static string? ParseSignatureResponse(string xml)
    {
        var root = XDocument.Parse(xml).Root;
        if (root is null)
            return null;

        var signature = root.Attribute("image") is not null
            ? root
            : root.Descendants("data").FirstOrDefault(e => e.Attribute("image") is not null);

        return Value(signature, "image");
    }

    /// <summary>
    /// Extracts the status code from a response. Returns 0 if the request was successful
    /// and -1 if the response carries no readable code.
    /// </summary>
    internal static int GetErrorCode(string xml)
        => ReadStatus(XDocument.Parse(xml).Root).Code;

    /// <summary>
    /// Extracts the error message (<c>error</c>, or <c>piece-status-desc</c> for public calls) from a response.
    /// </summary>
    internal static string? GetErrorMessage(string xml)
        => ReadStatus(XDocument.Parse(xml).Root).Message;

    /// <summary>
    /// Reads the request status. It is carried by <c>code</c>/<c>error</c> on the root, or on its first child
    /// (public responses put <c>code</c> on <c>piece-status-public-list</c>). Calls without a business contract
    /// ("NOLP") report errors as <c>piece-status</c>/<c>piece-status-desc</c> on <c>piece-status-public</c>;
    /// a non-zero value there is an error even when the list reports <c>code="0"</c>. For public success
    /// responses <c>piece-status</c> is <c>0</c>. <c>piece-status</c> of other elements is never read as an error.
    /// </summary>
    private static (int Code, string? Message) ReadStatus(XElement? root)
    {
        if (root is null)
            return (-1, null);

        var child = root.Elements("data").FirstOrDefault();
        var grandchild = child?.Elements("data").FirstOrDefault();

        var withCode = new[] { root, child }.OfType<XElement>().FirstOrDefault(e => Value(e, "code") is not null);
        var codeText = Value(withCode, "code");
        var message = Value(withCode, "error");

        var publicShipment = new[] { root, child, grandchild }.OfType<XElement>()
            .FirstOrDefault(e => IsNamed(e, PieceStatusPublic));
        var pieceStatus = Value(publicShipment, "piece-status");

        if (int.TryParse(pieceStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pieceStatusCode)
            && pieceStatusCode != 0
            && (codeText is null || codeText == "0"))
        {
            return (pieceStatusCode, Value(publicShipment, "piece-status-desc") ?? message);
        }

        if (codeText is null)
        {
            // No "code" anywhere: fall back to piece-status of the first child (or its first child).
            var fallback = new[] { child, grandchild }.OfType<XElement>()
                .FirstOrDefault(e => Value(e, "piece-status") is not null);
            codeText = Value(fallback, "piece-status");
            message ??= Value(fallback, "piece-status-desc");
        }

        message ??= Value(grandchild, "piece-status-desc") is { } desc && !IsSuccess(codeText) ? desc : null;

        return (int.TryParse(codeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code) ? code : -1, message);
    }

    private static bool IsSuccess(string? code) => code == "0";

    /// <summary>
    /// Locates the shipment element. <c>d-get-piece-detail</c> returns <c>piece-shipment</c> below
    /// <c>piece-shipment-list</c>; <c>get-status-for-public-user</c> returns <c>piece-status-public</c>
    /// below <c>piece-status-public-list</c>.
    /// </summary>
    private static XElement FindShipment(XElement root)
    {
        var shipment = FindByName(root, PieceShipment)
            ?? FindByName(FindByName(root, PieceStatusPublicList) ?? root, PieceStatusPublic);

        return shipment ?? root.Elements("data").FirstOrDefault() ?? root;
    }

    private static (List<TrackingEvent> Events, XElement? LastSourceElement) ParseEvents(XElement shipment)
    {
        var events = new List<TrackingEvent>();
        XElement? last = null;

        if (IsNamed(shipment, PieceStatusPublic))
        {
            // get-status-for-public-user: the shipment element itself describes the current event.
            events.Add(ParseSingleEvent(shipment));
            return (events, shipment);
        }

        // The event list is a child of piece-shipment in real responses and a sibling of it in the
        // documentation example; both layouts are accepted.
        var eventList = FindByName(shipment, PieceEventList)
            ?? shipment.Element(PieceEventList)
            ?? (shipment.Parent is { } parent
                ? parent.Elements("data").FirstOrDefault(e => IsNamed(e, PieceEventList))
                : null);

        if (eventList is not null)
        {
            var elements = eventList.Elements("data").Where(e => IsNamed(e, PieceEvent)).ToList();
            if (elements.Count == 0)
                elements = eventList.Elements(PieceEvent).ToList();

            foreach (var ev in elements)
            {
                events.Add(ParseSingleEvent(ev));
                last = ev;
            }
        }

        return (events, last);
    }

    private static TrackingStatus DetermineStatus(XElement shipment, XElement? lastEvent)
    {
        var ice = Value(shipment, "ice");
        var code = Value(shipment, "standard-event-code");
        var returned = Value(shipment, "ruecksendung");

        if (ice is null && code is null && lastEvent is not null)
        {
            ice = Value(lastEvent, "ice");
            code = Value(lastEvent, "standard-event-code");
            if (!string.Equals(returned, "true", StringComparison.OrdinalIgnoreCase))
                returned = Value(lastEvent, "ruecksendung") ?? returned;
        }

        return DhlTrackingStatusMapper.Map(Value(shipment, "delivery-event-flag"), returned, ice, code);
    }

    private static TrackingEvent ParseSingleEvent(XElement ev)
    {
        var timestamp = Value(ev, "event-timestamp")
            ?? Value(ev, "last-event-timestamp")
            ?? Value(ev, "status-timestamp");

        var locationParts = new[] { Value(ev, "event-location"), Value(ev, "event-country") }
            .Where(p => p is not null);

        return new TrackingEvent
        {
            Timestamp = DhlTrackingTimestamp.ParseTimestamp(timestamp),
            Location = string.Join(", ", locationParts) is { Length: > 0 } loc ? loc : null,
            Description = Value(ev, "event-text") ?? Value(ev, "event-status") ?? Value(ev, "status") ?? "Unknown",
            StatusCode = Value(ev, "standard-event-code") ?? Value(ev, "event-short-status")
        };
    }

    /// <summary>
    /// Returns the trimmed attribute value, or null when the attribute is missing or blank.
    /// </summary>
    private static string? Value(XElement? element, string attribute)
    {
        var value = element?.Attribute(attribute)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsNamed(XElement element, string name)
        => element.Name.LocalName == "data" && element.Attribute("name")?.Value == name;

    /// <summary>
    /// Finds a child &lt;data&gt; element whose <c>name</c> attribute matches the given value.
    /// </summary>
    private static XElement? FindByName(XElement parent, string name)
        => parent.Elements("data").FirstOrDefault(e => IsNamed(e, name));
}
