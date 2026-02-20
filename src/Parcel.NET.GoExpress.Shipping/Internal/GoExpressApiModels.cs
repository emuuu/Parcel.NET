using System.Text.Json.Serialization;

namespace Parcel.NET.GoExpress.Shipping.Internal;

// --- Request Models ---

internal class GoExpressOrderRequest
{
    [JsonPropertyName("responsibleStation")]
    public string? ResponsibleStation { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("shipment")]
    public required GoExpressApiShipment Shipment { get; set; }

    [JsonPropertyName("consignorAddress")]
    public required GoExpressApiAddress ConsignorAddress { get; set; }

    [JsonPropertyName("neutralAddress")]
    public GoExpressApiAddress? NeutralAddress { get; set; }

    [JsonPropertyName("consigneeAddress")]
    public required GoExpressApiAddress ConsigneeAddress { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("packages")]
    public required List<GoExpressApiPackage> Packages { get; set; }
}

internal class GoExpressApiShipment
{
    [JsonPropertyName("hwbNumber")]
    public string? HwbNumber { get; set; }

    [JsonPropertyName("orderStatus")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("validation")]
    public string? Validation { get; set; }

    [JsonPropertyName("service")]
    public required string Service { get; set; }

    [JsonPropertyName("weight")]
    public required string Weight { get; set; }

    [JsonPropertyName("packageCount")]
    public required string PackageCount { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("customerReference")]
    public string? CustomerReference { get; set; }

    [JsonPropertyName("costCenter")]
    public string? CostCenter { get; set; }

    [JsonPropertyName("selfPickup")]
    public string? SelfPickup { get; set; }

    [JsonPropertyName("selfDelivery")]
    public string? SelfDelivery { get; set; }

    [JsonPropertyName("dimensions")]
    public string? Dimensions { get; set; }

    [JsonPropertyName("freightCollect")]
    public string? FreightCollect { get; set; }

    [JsonPropertyName("identCheck")]
    public string? IdentCheck { get; set; }

    [JsonPropertyName("receiptNotice")]
    public string? ReceiptNotice { get; set; }

    [JsonPropertyName("isNeutralPickup")]
    public string? IsNeutralPickup { get; set; }

    [JsonPropertyName("pickup")]
    public required GoExpressApiTimeWindow Pickup { get; set; }

    [JsonPropertyName("delivery")]
    public GoExpressApiTimeWindow? Delivery { get; set; }

    [JsonPropertyName("insurance")]
    public GoExpressApiMoney? Insurance { get; set; }

    [JsonPropertyName("valueOfGoods")]
    public GoExpressApiMoney? ValueOfGoods { get; set; }

    [JsonPropertyName("cashOnDelivery")]
    public GoExpressApiMoney? CashOnDelivery { get; set; }
}

internal class GoExpressApiAddress
{
    [JsonPropertyName("name1")]
    public string? Name1 { get; set; }

    [JsonPropertyName("name2")]
    public string? Name2 { get; set; }

    [JsonPropertyName("name3")]
    public string? Name3 { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("houseNumber")]
    public string? HouseNumber { get; set; }

    [JsonPropertyName("zipCode")]
    public string? ZipCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("telephoneAvis")]
    public string? TelephoneAvis { get; set; }

    [JsonPropertyName("deliveryCode")]
    public string? DeliveryCode { get; set; }

    [JsonPropertyName("deliveryCodeEncryption")]
    public string? DeliveryCodeEncryption { get; set; }
}

internal class GoExpressApiPackage
{
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
    public string? Date { get; set; }

    [JsonPropertyName("timeFrom")]
    public string? TimeFrom { get; set; }

    [JsonPropertyName("timeTill")]
    public string? TimeTill { get; set; }

    [JsonPropertyName("avisFrom")]
    public string? AvisFrom { get; set; }

    [JsonPropertyName("avisTill")]
    public string? AvisTill { get; set; }

    [JsonPropertyName("weekendOrHolidayIndicator")]
    public string? WeekendOrHolidayIndicator { get; set; }
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

    [JsonPropertyName("orderStatus")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("pickupDate")]
    public string? PickupDate { get; set; }

    [JsonPropertyName("deliveryDate")]
    public string? DeliveryDate { get; set; }

    [JsonPropertyName("transitInfo")]
    public GoExpressApiTransitInfo? TransitInfo { get; set; }

    [JsonPropertyName("hwbOrPackageLabel")]
    public string? HwbOrPackageLabel { get; set; }

    [JsonPropertyName("package")]
    public List<GoExpressApiPackageBarcode>? Package { get; set; }
}

internal class GoExpressApiTransitInfo
{
    [JsonPropertyName("datesVerified")]
    public string? DatesVerified { get; set; }

    [JsonPropertyName("addressesVerified")]
    public string? AddressesVerified { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}

internal class GoExpressApiPackageBarcode
{
    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }
}

// --- Update Status Models ---

internal class GoExpressUpdateStatusRequest
{
    [JsonPropertyName("responsibleStation")]
    public string? ResponsibleStation { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("hwbNumber")]
    public required string HwbNumber { get; set; }

    [JsonPropertyName("orderStatus")]
    public required string OrderStatus { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

// --- Label Request Models ---

internal class GoExpressLabelRequest
{
    [JsonPropertyName("responsibleStation")]
    public string? ResponsibleStation { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("hwb")]
    public required string Hwb { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

