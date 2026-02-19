using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Returns.Internal;

// ── POST /orders request ──

internal class DhlReturnOrderRequest
{
    [JsonPropertyName("receiverId")]
    public required string ReceiverId { get; set; }

    [JsonPropertyName("customerReference")]
    public string? CustomerReference { get; set; }

    [JsonPropertyName("shipper")]
    public required DhlReturnShipper Shipper { get; set; }

    [JsonPropertyName("itemWeight")]
    public DhlReturnWeight? ItemWeight { get; set; }

    [JsonPropertyName("itemValue")]
    public DhlReturnValue? ItemValue { get; set; }

    [JsonPropertyName("customsDetails")]
    public DhlReturnCustomsDetails? CustomsDetails { get; set; }
}

internal class DhlReturnShipper
{
    [JsonPropertyName("name1")]
    public required string Name1 { get; set; }

    [JsonPropertyName("name2")]
    public string? Name2 { get; set; }

    [JsonPropertyName("name3")]
    public string? Name3 { get; set; }

    [JsonPropertyName("addressStreet")]
    public required string AddressStreet { get; set; }

    [JsonPropertyName("addressHouse")]
    public string? AddressHouse { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("additionalAddressInformation1")]
    public string? AdditionalAddressInformation1 { get; set; }

    [JsonPropertyName("additionalAddressInformation2")]
    public string? AdditionalAddressInformation2 { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}

internal class DhlReturnWeight
{
    [JsonPropertyName("uom")]
    public required string Uom { get; set; }

    [JsonPropertyName("value")]
    public required int Value { get; set; }
}

internal class DhlReturnValue
{
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [JsonPropertyName("value")]
    public required decimal Value { get; set; }
}

internal class DhlReturnCustomsDetails
{
    [JsonPropertyName("items")]
    public required List<DhlReturnCustomsItem> Items { get; set; }
}

internal class DhlReturnCustomsItem
{
    [JsonPropertyName("itemDescription")]
    public required string ItemDescription { get; set; }

    [JsonPropertyName("packagedQuantity")]
    public required int PackagedQuantity { get; set; }

    [JsonPropertyName("itemWeight")]
    public required DhlReturnWeight ItemWeight { get; set; }

    [JsonPropertyName("itemValue")]
    public required DhlReturnValue ItemValue { get; set; }

    [JsonPropertyName("countryOfOrigin")]
    public required string CountryOfOrigin { get; set; }

    [JsonPropertyName("hsCode")]
    public string? HsCode { get; set; }
}

// ── POST /orders response ──

internal class DhlReturnOrderResponse
{
    [JsonPropertyName("shipmentNo")]
    public string? ShipmentNo { get; set; }

    [JsonPropertyName("internationalShipmentNo")]
    public string? InternationalShipmentNo { get; set; }

    [JsonPropertyName("label")]
    public DhlReturnLabelData? Label { get; set; }

    [JsonPropertyName("qrLabel")]
    public DhlReturnLabelData? QrLabel { get; set; }

    [JsonPropertyName("routingCode")]
    public string? RoutingCode { get; set; }

    [JsonPropertyName("status")]
    public DhlReturnStatus? Status { get; set; }
}

internal class DhlReturnLabelData
{
    [JsonPropertyName("b64")]
    public string? B64 { get; set; }
}

internal class DhlReturnStatus
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

// ── GET /locations response ──

internal class DhlReturnLocationItem
{
    [JsonPropertyName("receiverId")]
    public string? ReceiverId { get; set; }

    [JsonPropertyName("billingNumber")]
    public string? BillingNumber { get; set; }

    [JsonPropertyName("address")]
    public DhlReturnLocationAddress? Address { get; set; }
}

internal class DhlReturnLocationAddress
{
    [JsonPropertyName("name1")]
    public string? Name1 { get; set; }

    [JsonPropertyName("addressStreet")]
    public string? AddressStreet { get; set; }

    [JsonPropertyName("addressHouse")]
    public string? AddressHouse { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
