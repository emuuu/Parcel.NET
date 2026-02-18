using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Tracking.Internal;

internal class DhlTrackingResponse
{
    [JsonPropertyName("shipments")]
    public List<DhlTrackedShipment>? Shipments { get; set; }
}

internal class DhlTrackedShipment
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("status")]
    public DhlTrackingStatus? Status { get; set; }

    [JsonPropertyName("estimatedTimeOfDelivery")]
    public string? EstimatedTimeOfDelivery { get; set; }

    [JsonPropertyName("events")]
    public List<DhlTrackingEvent>? Events { get; set; }
}

internal class DhlTrackingStatus
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("location")]
    public DhlTrackingLocation? Location { get; set; }
}

internal class DhlTrackingEvent
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("location")]
    public DhlTrackingLocation? Location { get; set; }
}

internal class DhlTrackingLocation
{
    [JsonPropertyName("address")]
    public DhlTrackingAddress? Address { get; set; }
}

internal class DhlTrackingAddress
{
    [JsonPropertyName("addressLocality")]
    public string? AddressLocality { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}
