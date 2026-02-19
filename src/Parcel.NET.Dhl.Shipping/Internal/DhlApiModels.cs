using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Shipping.Internal;

internal class DhlOrderRequest
{
    [JsonPropertyName("profile")]
    public required string Profile { get; set; }

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

    [JsonPropertyName("costCenter")]
    public string? CostCenter { get; set; }

    [JsonPropertyName("creationSoftware")]
    public string? CreationSoftware { get; set; }

    [JsonPropertyName("shipDate")]
    public string? ShipDate { get; set; }

    [JsonPropertyName("shipper")]
    public required DhlApiShipper Shipper { get; set; }

    [JsonPropertyName("consignee")]
    public required object Consignee { get; set; }

    [JsonPropertyName("details")]
    public required DhlShipmentDetails Details { get; set; }

    [JsonPropertyName("services")]
    public DhlApiServices? Services { get; set; }

    [JsonPropertyName("customs")]
    public DhlApiCustomsDetails? Customs { get; set; }
}

// Shipper address — API schema: Shipper
internal class DhlApiShipper
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

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }
}

// Consignee: ContactAddress — standard address delivery
internal class DhlApiContactAddress
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

    [JsonPropertyName("additionalAddressInformation1")]
    public string? AdditionalAddressInformation1 { get; set; }

    [JsonPropertyName("additionalAddressInformation2")]
    public string? AdditionalAddressInformation2 { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("contactName")]
    public string? ContactName { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("dispatchingInformation")]
    public string? DispatchingInformation { get; set; }
}

// Consignee: Locker (Packstation)
internal class DhlApiLocker
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("lockerID")]
    public required int LockerID { get; set; }

    [JsonPropertyName("postNumber")]
    public required string PostNumber { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

// Consignee: PostOffice (Filiale)
internal class DhlApiPostOffice
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("retailID")]
    public required int RetailID { get; set; }

    [JsonPropertyName("postNumber")]
    public string? PostNumber { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

// Consignee: POBox (Postfach)
internal class DhlApiPOBox
{
    [JsonPropertyName("name1")]
    public required string Name1 { get; set; }

    [JsonPropertyName("poBoxID")]
    public required int PoBoxID { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
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
    public int Height { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }
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

    [JsonPropertyName("namedPersonOnly")]
    public bool? NamedPersonOnly { get; set; }

    [JsonPropertyName("noNeighbourDelivery")]
    public bool? NoNeighbourDelivery { get; set; }

    [JsonPropertyName("signedForByRecipient")]
    public bool? SignedForByRecipient { get; set; }

    [JsonPropertyName("premium")]
    public bool? Premium { get; set; }

    [JsonPropertyName("closestDropPoint")]
    public bool? ClosestDropPoint { get; set; }

    [JsonPropertyName("endorsement")]
    public string? Endorsement { get; set; }

    [JsonPropertyName("visualCheckOfAge")]
    public string? VisualCheckOfAge { get; set; }

    [JsonPropertyName("parcelOutletRouting")]
    public string? ParcelOutletRouting { get; set; }

    [JsonPropertyName("additionalInsurance")]
    public DhlApiMonetaryValue? AdditionalInsurance { get; set; }

    [JsonPropertyName("cashOnDelivery")]
    public DhlApiCashOnDelivery? CashOnDelivery { get; set; }

    [JsonPropertyName("identCheck")]
    public DhlApiIdentCheck? IdentCheck { get; set; }

    [JsonPropertyName("dhlRetoure")]
    public DhlApiRetoure? DhlRetoure { get; set; }

    [JsonPropertyName("postalDeliveryDutyPaid")]
    public bool? PostalDeliveryDutyPaid { get; set; }
}

internal class DhlApiCashOnDelivery
{
    [JsonPropertyName("amount")]
    public required DhlApiMonetaryValue Amount { get; set; }

    [JsonPropertyName("bankAccount")]
    public DhlApiBankAccount? BankAccount { get; set; }

    [JsonPropertyName("accountReference")]
    public string? AccountReference { get; set; }

    [JsonPropertyName("transferNote1")]
    public string? TransferNote1 { get; set; }

    [JsonPropertyName("transferNote2")]
    public string? TransferNote2 { get; set; }
}

internal class DhlApiBankAccount
{
    [JsonPropertyName("accountHolder")]
    public required string AccountHolder { get; set; }

    [JsonPropertyName("iban")]
    public required string Iban { get; set; }

    [JsonPropertyName("bic")]
    public string? Bic { get; set; }
}

internal class DhlApiIdentCheck
{
    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public required string DateOfBirth { get; set; }

    [JsonPropertyName("minimumAge")]
    public string? MinimumAge { get; set; }
}

internal class DhlApiRetoure
{
    [JsonPropertyName("billingNumber")]
    public required string BillingNumber { get; set; }

    [JsonPropertyName("returnAddress")]
    public DhlApiShipper? ReturnAddress { get; set; }
}

internal class DhlApiMonetaryValue
{
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

// Customs for international shipments
internal class DhlApiCustomsDetails
{
    [JsonPropertyName("invoiceNo")]
    public string? InvoiceNo { get; set; }

    [JsonPropertyName("exportType")]
    public required string ExportType { get; set; }

    [JsonPropertyName("exportDescription")]
    public string? ExportDescription { get; set; }

    [JsonPropertyName("postalCharges")]
    public DhlApiMonetaryValue? PostalCharges { get; set; }

    [JsonPropertyName("shippingConditions")]
    public string? ShippingConditions { get; set; }

    [JsonPropertyName("items")]
    public required List<DhlApiCustomsItem> Items { get; set; }
}

internal class DhlApiCustomsItem
{
    [JsonPropertyName("itemDescription")]
    public required string ItemDescription { get; set; }

    [JsonPropertyName("countryOfOrigin")]
    public required string CountryOfOrigin { get; set; }

    [JsonPropertyName("hsCode")]
    public string? HsCode { get; set; }

    [JsonPropertyName("packagedQuantity")]
    public int PackagedQuantity { get; set; }

    [JsonPropertyName("itemWeight")]
    public required DhlApiWeight ItemWeight { get; set; }

    [JsonPropertyName("itemValue")]
    public required DhlApiMonetaryValue ItemValue { get; set; }
}

// Manifest request
internal class DhlManifestRequest
{
    [JsonPropertyName("profile")]
    public required string Profile { get; set; }

    [JsonPropertyName("shipmentNumbers")]
    public List<string>? ShipmentNumbers { get; set; }

    [JsonPropertyName("billingNumber")]
    public string? BillingNumber { get; set; }
}

// Response models
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

    [JsonPropertyName("returnShipmentNo")]
    public string? ReturnShipmentNo { get; set; }

    [JsonPropertyName("shipmentRefNo")]
    public string? ShipmentRefNo { get; set; }

    [JsonPropertyName("routingCode")]
    public string? RoutingCode { get; set; }

    [JsonPropertyName("label")]
    public DhlApiDocument? Label { get; set; }

    [JsonPropertyName("returnLabel")]
    public DhlApiDocument? ReturnLabel { get; set; }

    [JsonPropertyName("customsDoc")]
    public DhlApiDocument? CustomsDoc { get; set; }

    [JsonPropertyName("codLabel")]
    public DhlApiDocument? CodLabel { get; set; }

    [JsonPropertyName("sstatus")]
    public DhlApiStatus? Status { get; set; }

    [JsonPropertyName("validationMessages")]
    public List<DhlApiValidationMessage>? ValidationMessages { get; set; }
}

internal class DhlApiDocument
{
    [JsonPropertyName("b64")]
    public string? B64 { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("zpl2")]
    public string? Zpl2 { get; set; }

    [JsonPropertyName("fileFormat")]
    public string? FileFormat { get; set; }

    [JsonPropertyName("printFormat")]
    public string? PrintFormat { get; set; }
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
