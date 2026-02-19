using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.UnifiedTracking.Internal;

internal class DhlUnifiedTrackingResponse
{
    [JsonPropertyName("shipments")]
    public List<DhlUnifiedTrackedShipment>? Shipments { get; set; }
}

internal class DhlUnifiedTrackedShipment
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("status")]
    public DhlUnifiedTrackingStatus? Status { get; set; }

    [JsonPropertyName("estimatedTimeOfDelivery")]
    public string? EstimatedTimeOfDelivery { get; set; }

    [JsonPropertyName("events")]
    public List<DhlUnifiedTrackingEvent>? Events { get; set; }
}

internal class DhlUnifiedTrackingStatus
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
    public DhlUnifiedTrackingLocation? Location { get; set; }
}

internal class DhlUnifiedTrackingEvent
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
    public DhlUnifiedTrackingLocation? Location { get; set; }
}

internal class DhlUnifiedTrackingLocation
{
    [JsonPropertyName("address")]
    public DhlUnifiedTrackingAddress? Address { get; set; }
}

internal class DhlUnifiedTrackingAddress
{
    [JsonPropertyName("addressLocality")]
    public string? AddressLocality { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}
