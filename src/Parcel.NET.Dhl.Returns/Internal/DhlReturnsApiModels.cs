using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Returns.Internal;

internal class DhlReturnOrderRequest
{
    [JsonPropertyName("receiverId")]
    public required string ReceiverId { get; set; }

    [JsonPropertyName("senderAddress")]
    public required DhlReturnSenderAddress SenderAddress { get; set; }

    [JsonPropertyName("shipmentReference")]
    public string? ShipmentReference { get; set; }

    [JsonPropertyName("returnDocumentType")]
    public string? ReturnDocumentType { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("telephoneNumber")]
    public string? TelephoneNumber { get; set; }

    [JsonPropertyName("weightInGrams")]
    public int? WeightInGrams { get; set; }

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }
}

internal class DhlReturnSenderAddress
{
    [JsonPropertyName("name1")]
    public required string Name1 { get; set; }

    [JsonPropertyName("streetName")]
    public required string StreetName { get; set; }

    [JsonPropertyName("houseNumber")]
    public string? HouseNumber { get; set; }

    [JsonPropertyName("postCode")]
    public required string PostCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required DhlReturnCountry Country { get; set; }
}

internal class DhlReturnCountry
{
    [JsonPropertyName("countryISOCode")]
    public required string CountryIsoCode { get; set; }
}

internal class DhlReturnOrderResponse
{
    [JsonPropertyName("shipmentNumber")]
    public string? ShipmentNumber { get; set; }

    [JsonPropertyName("labelData")]
    public string? LabelData { get; set; }

    [JsonPropertyName("labelUrl")]
    public string? LabelUrl { get; set; }

    [JsonPropertyName("qrLabelData")]
    public string? QrLabelData { get; set; }

    [JsonPropertyName("routingCode")]
    public string? RoutingCode { get; set; }
}

internal class DhlReturnLocationResponse
{
    [JsonPropertyName("locations")]
    public List<DhlReturnLocationItem>? Locations { get; set; }
}

internal class DhlReturnLocationItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}
