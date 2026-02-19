using System.Text.Json.Serialization;

namespace ParcelNET.GoExpress.Shipping.Internal;

// --- Request Models ---

internal class GoExpressOrderRequest
{
    [JsonPropertyName("responsibleStation")]
    public string? ResponsibleStation { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("shipment")]
    public required GoExpressApiShipment Shipment { get; set; }
}

internal class GoExpressApiShipment
{
    [JsonPropertyName("service")]
    public required string Service { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("costCenter")]
    public string? CostCenter { get; set; }

    [JsonPropertyName("labelFormat")]
    public string? LabelFormat { get; set; }

    [JsonPropertyName("selfPickup")]
    public string? SelfPickup { get; set; }

    [JsonPropertyName("selfDelivery")]
    public string? SelfDelivery { get; set; }

    [JsonPropertyName("freightCollect")]
    public string? FreightCollect { get; set; }

    [JsonPropertyName("identCheck")]
    public string? IdentCheck { get; set; }

    [JsonPropertyName("receiptNotice")]
    public string? ReceiptNotice { get; set; }

    [JsonPropertyName("insurance")]
    public GoExpressApiMoney? Insurance { get; set; }

    [JsonPropertyName("cashOnDelivery")]
    public GoExpressApiMoney? CashOnDelivery { get; set; }

    [JsonPropertyName("valueOfGoods")]
    public GoExpressApiMoney? ValueOfGoods { get; set; }

    [JsonPropertyName("pickup")]
    public required GoExpressApiTimeWindow Pickup { get; set; }

    [JsonPropertyName("delivery")]
    public GoExpressApiTimeWindow? Delivery { get; set; }

    [JsonPropertyName("shipper")]
    public required GoExpressApiAddress Shipper { get; set; }

    [JsonPropertyName("consignee")]
    public required GoExpressApiAddress Consignee { get; set; }

    [JsonPropertyName("packages")]
    public required List<GoExpressApiPackage> Packages { get; set; }
}

internal class GoExpressApiAddress
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("houseNumber")]
    public string? HouseNumber { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("countryCode")]
    public required string CountryCode { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}

internal class GoExpressApiPackage
{
    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("length")]
    public string? Length { get; set; }

    [JsonPropertyName("width")]
    public string? Width { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }
}

internal class GoExpressApiTimeWindow
{
    [JsonPropertyName("date")]
    public required string Date { get; set; }

    [JsonPropertyName("timeFrom")]
    public string? TimeFrom { get; set; }

    [JsonPropertyName("timeTill")]
    public string? TimeTill { get; set; }

    [JsonPropertyName("isWeekend")]
    public string? IsWeekend { get; set; }

    [JsonPropertyName("isHoliday")]
    public string? IsHoliday { get; set; }
}

internal class GoExpressApiMoney
{
    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

// --- Response Models ---

internal class GoExpressOrderResponse
{
    [JsonPropertyName("hwbNumber")]
    public string? HwbNumber { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("transit")]
    public GoExpressApiTransitInfo? Transit { get; set; }

    [JsonPropertyName("packages")]
    public List<GoExpressApiPackageInner>? Packages { get; set; }
}

internal class GoExpressApiTransitInfo
{
    [JsonPropertyName("estimatedDelivery")]
    public string? EstimatedDelivery { get; set; }
}

internal class GoExpressApiPackageInner
{
    [JsonPropertyName("packageNumber")]
    public string? PackageNumber { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }
}

// --- Update Status Models ---

internal class GoExpressUpdateStatusRequest
{
    [JsonPropertyName("hwbNumber")]
    public required string HwbNumber { get; set; }

    [JsonPropertyName("orderStatus")]
    public required string OrderStatus { get; set; }
}

// --- Label Request Models ---

internal class GoExpressLabelRequest
{
    [JsonPropertyName("hwbNumber")]
    public required string HwbNumber { get; set; }

    [JsonPropertyName("labelFormat")]
    public string? LabelFormat { get; set; }
}

internal class GoExpressLabelResponse
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
