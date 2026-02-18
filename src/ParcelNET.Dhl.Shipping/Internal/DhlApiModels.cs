using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Shipping.Internal;

internal class DhlOrderRequest
{
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    [JsonPropertyName("shipments")]
    public required List<DhlShipment> Shipments { get; set; }
}

internal class DhlShipment
{
    [JsonPropertyName("product")]
    public required string Product { get; set; }

    [JsonPropertyName("billingNumber")]
    public required string BillingNumber { get; set; }

    [JsonPropertyName("refNo")]
    public string? RefNo { get; set; }

    [JsonPropertyName("shipDate")]
    public string? ShipDate { get; set; }

    [JsonPropertyName("shipper")]
    public required DhlApiAddress Shipper { get; set; }

    [JsonPropertyName("consignee")]
    public required DhlApiAddress Consignee { get; set; }

    [JsonPropertyName("details")]
    public required DhlShipmentDetails Details { get; set; }

    [JsonPropertyName("services")]
    public DhlApiServices? Services { get; set; }
}

internal class DhlApiAddress
{
    [JsonPropertyName("name1")]
    public required string Name1 { get; set; }

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

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }
}

internal class DhlShipmentDetails
{
    [JsonPropertyName("dim")]
    public DhlApiDimensions? Dim { get; set; }

    [JsonPropertyName("weight")]
    public required DhlApiWeight Weight { get; set; }
}

internal class DhlApiDimensions
{
    [JsonPropertyName("uom")]
    public string Uom { get; set; } = "cm";

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("length")]
    public double Length { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }
}

internal class DhlApiWeight
{
    [JsonPropertyName("uom")]
    public string Uom { get; set; } = "kg";

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

internal class DhlApiServices
{
    [JsonPropertyName("preferredDay")]
    public string? PreferredDay { get; set; }

    [JsonPropertyName("preferredLocation")]
    public string? PreferredLocation { get; set; }

    [JsonPropertyName("preferredNeighbour")]
    public string? PreferredNeighbour { get; set; }

    [JsonPropertyName("bulkyGoods")]
    public bool? BulkyGoods { get; set; }

    [JsonPropertyName("additionalInsurance")]
    public DhlApiMonetaryValue? AdditionalInsurance { get; set; }

    [JsonPropertyName("cashOnDelivery")]
    public DhlApiMonetaryValue? CashOnDelivery { get; set; }
}

internal class DhlApiMonetaryValue
{
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

internal class DhlOrderResponse
{
    [JsonPropertyName("status")]
    public required DhlApiStatus Status { get; set; }

    [JsonPropertyName("items")]
    public List<DhlOrderItem>? Items { get; set; }
}

internal class DhlOrderItem
{
    [JsonPropertyName("shipmentNo")]
    public string? ShipmentNo { get; set; }

    [JsonPropertyName("label")]
    public DhlApiLabel? Label { get; set; }

    [JsonPropertyName("sstatus")]
    public DhlApiStatus? Status { get; set; }

    [JsonPropertyName("validationMessages")]
    public List<DhlApiValidationMessage>? ValidationMessages { get; set; }
}

internal class DhlApiLabel
{
    [JsonPropertyName("b64")]
    public string? B64 { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal class DhlApiStatus
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

internal class DhlApiValidationMessage
{
    [JsonPropertyName("property")]
    public string? Property { get; set; }

    [JsonPropertyName("validationMessage")]
    public string? Message { get; set; }

    [JsonPropertyName("validationState")]
    public string? ValidationState { get; set; }
}
