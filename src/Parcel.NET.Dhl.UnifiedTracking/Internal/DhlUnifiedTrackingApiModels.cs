using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.UnifiedTracking.Internal;

internal class DhlUnifiedTrackingResponse
{
    [JsonPropertyName("shipments")]
    public List<DhlUnifiedTrackedShipment>? Shipments { get; set; }
}

internal class DhlUnifiedTrackedShipment
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("service")]
    public string? Service { get; set; }

    [JsonPropertyName("status")]
    public DhlUnifiedTrackingStatus? Status { get; set; }

    [JsonPropertyName("estimatedTimeOfDelivery")]
    public string? EstimatedTimeOfDelivery { get; set; }

    [JsonPropertyName("estimatedDeliveryTimeFrame")]
    public DhlUnifiedTimeFrame? EstimatedDeliveryTimeFrame { get; set; }

    [JsonPropertyName("estimatedTimeOfDeliveryRemark")]
    public string? EstimatedTimeOfDeliveryRemark { get; set; }

    [JsonPropertyName("origin")]
    public DhlUnifiedTrackingLocation? Origin { get; set; }

    [JsonPropertyName("destination")]
    public DhlUnifiedTrackingLocation? Destination { get; set; }

    [JsonPropertyName("details")]
    public DhlUnifiedShipmentDetails? Details { get; set; }

    [JsonPropertyName("proofOfDelivery")]
    public DhlUnifiedProofOfDelivery? ProofOfDelivery { get; set; }

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

    [JsonPropertyName("remark")]
    public string? Remark { get; set; }

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

    [JsonPropertyName("remark")]
    public string? Remark { get; set; }

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

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

internal class DhlUnifiedTimeFrame
{
    [JsonPropertyName("estimatedFrom")]
    public string? EstimatedFrom { get; set; }

    [JsonPropertyName("estimatedThrough")]
    public string? EstimatedThrough { get; set; }
}

internal class DhlUnifiedShipmentDetails
{
    [JsonPropertyName("weight")]
    public DhlUnifiedWeight? Weight { get; set; }

    [JsonPropertyName("proofOfDeliverySignedAvailable")]
    public bool? ProofOfDeliverySignedAvailable { get; set; }

    [JsonPropertyName("totalNumberOfPieces")]
    public int? TotalNumberOfPieces { get; set; }
}

internal class DhlUnifiedWeight
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("unitText")]
    public string? UnitText { get; set; }
}

internal class DhlUnifiedProofOfDelivery
{
    [JsonPropertyName("documentUrl")]
    public string? DocumentUrl { get; set; }

    [JsonPropertyName("signatureUrl")]
    public string? SignatureUrl { get; set; }
}
