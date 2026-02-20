using System.Text.Json.Serialization;

namespace Parcel.NET.GoExpress.Tracking.Internal;

internal class GoExpressTrackingResponse
{
    [JsonPropertyName("trackingItems")]
    public GoExpressTrackingItems? TrackingItems { get; set; }
}

internal class GoExpressTrackingItems
{
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("recipientZipCode")]
    public string? RecipientZipCode { get; set; }

    [JsonPropertyName("transportStation")]
    public GoExpressTransportStation? TransportStation { get; set; }

    [JsonPropertyName("transportStatus")]
    public string? TransportStatus { get; set; }

    [JsonPropertyName("locationInfo")]
    public string? LocationInfo { get; set; }

    [JsonPropertyName("locationOverview")]
    public List<GoExpressLocationOverview>? LocationOverview { get; set; }

    [JsonPropertyName("trackingTable")]
    public List<GoExpressTrackingTableEntry>? TrackingTable { get; set; }
}

internal class GoExpressTransportStation
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }
}

internal class GoExpressLocationOverview
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("trackingStatuses")]
    public List<GoExpressTrackingStatusEntry>? TrackingStatuses { get; set; }
}

internal class GoExpressTrackingStatusEntry
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("statusDate")]
    public string? StatusDate { get; set; }
}

internal class GoExpressTrackingTableEntry
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("statusDate")]
    public string? StatusDate { get; set; }

    [JsonPropertyName("station")]
    public string? Station { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}
