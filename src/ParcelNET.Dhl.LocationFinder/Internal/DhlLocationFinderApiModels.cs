using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.LocationFinder.Internal;

internal class DhlLocationFinderResponse
{
    [JsonPropertyName("locations")]
    public List<DhlLocationFinderItem>? Locations { get; set; }
}

internal class DhlLocationFinderItem
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("location")]
    public DhlLocationFinderDetail? Location { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("distance")]
    public double? Distance { get; set; }

    [JsonPropertyName("place")]
    public DhlLocationFinderPlace? Place { get; set; }

    [JsonPropertyName("serviceTypes")]
    public List<string>? ServiceTypes { get; set; }

    [JsonPropertyName("openingHours")]
    public List<DhlLocationFinderOpeningHours>? OpeningHours { get; set; }
}

internal class DhlLocationFinderDetail
{
    [JsonPropertyName("ids")]
    public List<DhlLocationFinderId>? Ids { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [JsonPropertyName("keywordId")]
    public string? KeywordId { get; set; }
}

internal class DhlLocationFinderId
{
    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}

internal class DhlLocationFinderPlace
{
    [JsonPropertyName("address")]
    public DhlLocationFinderAddress? Address { get; set; }

    [JsonPropertyName("geo")]
    public DhlLocationFinderGeo? Geo { get; set; }
}

internal class DhlLocationFinderAddress
{
    [JsonPropertyName("streetAddress")]
    public string? StreetAddress { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("addressLocality")]
    public string? AddressLocality { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

internal class DhlLocationFinderGeo
{
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}

internal class DhlLocationFinderOpeningHours
{
    [JsonPropertyName("opens")]
    public string? Opens { get; set; }

    [JsonPropertyName("closes")]
    public string? Closes { get; set; }

    [JsonPropertyName("dayOfWeek")]
    public string? DayOfWeek { get; set; }
}

internal class DhlLocationFinderSingleResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("location")]
    public DhlLocationFinderDetail? Location { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("place")]
    public DhlLocationFinderPlace? Place { get; set; }

    [JsonPropertyName("serviceTypes")]
    public List<string>? ServiceTypes { get; set; }

    [JsonPropertyName("openingHours")]
    public List<DhlLocationFinderOpeningHours>? OpeningHours { get; set; }
}
