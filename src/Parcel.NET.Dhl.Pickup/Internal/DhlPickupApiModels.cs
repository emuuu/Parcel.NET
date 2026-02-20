using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Pickup.Internal;

// ──────────────────────────────────────────────────────────────
//  POST /orders — Request
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Internal request model for DHL Pickup API v3 POST /orders.
/// </summary>
internal class DhlPickupOrderRequest
{
    [JsonPropertyName("customerDetails")]
    public required DhlPickupCustomerDetails CustomerDetails { get; set; }

    [JsonPropertyName("pickupLocation")]
    public required DhlPickupLocation PickupLocation { get; set; }

    [JsonPropertyName("businessHours")]
    public DhlTimeFrame[]? BusinessHours { get; set; }

    [JsonPropertyName("contactPerson")]
    public DhlContactPerson[]? ContactPerson { get; set; }

    [JsonPropertyName("pickupDetails")]
    public required DhlPickupDetails PickupDetails { get; set; }

    [JsonPropertyName("shipmentDetails")]
    public required DhlShipmentDetails ShipmentDetails { get; set; }
}

internal class DhlPickupCustomerDetails
{
    [JsonPropertyName("billingNumber")]
    public required string BillingNumber { get; set; }
}

/// <summary>
/// Pickup location — oneOf Address or Id.
/// </summary>
internal class DhlPickupLocation
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Used when Type == "Address".
    /// </summary>
    [JsonPropertyName("pickupAddress")]
    public DhlPickupAddress? PickupAddress { get; set; }

    /// <summary>
    /// Used when Type == "Id".
    /// </summary>
    [JsonPropertyName("asId")]
    public string? AsId { get; set; }
}

internal class DhlPickupAddress
{
    [JsonPropertyName("name1")]
    public required string Name1 { get; set; }

    [JsonPropertyName("name2")]
    public string? Name2 { get; set; }

    [JsonPropertyName("addressStreet")]
    public required string AddressStreet { get; set; }

    [JsonPropertyName("addressHouse")]
    public required string AddressHouse { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

internal class DhlTimeFrame
{
    [JsonPropertyName("timeFrom")]
    public required string TimeFrom { get; set; }

    [JsonPropertyName("timeUntil")]
    public required string TimeUntil { get; set; }
}

internal class DhlContactPerson
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("emailNotification")]
    public DhlEmailNotification? EmailNotification { get; set; }
}

internal class DhlEmailNotification
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

internal class DhlPickupDetails
{
    [JsonPropertyName("pickupDate")]
    public required DhlPickupDate PickupDate { get; set; }

    [JsonPropertyName("totalWeight")]
    public DhlWeight? TotalWeight { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

internal class DhlPickupDate
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Date value in yyyy-MM-dd format. Used when Type == "Date".
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

internal class DhlWeight
{
    [JsonPropertyName("uom")]
    public required string Uom { get; set; }

    [JsonPropertyName("value")]
    public required double Value { get; set; }
}

internal class DhlShipmentDetails
{
    [JsonPropertyName("shipments")]
    public required DhlShipment[] Shipments { get; set; }
}

internal class DhlShipment
{
    [JsonPropertyName("transportationType")]
    public required string TransportationType { get; set; }

    [JsonPropertyName("replacement")]
    public bool? Replacement { get; set; }

    [JsonPropertyName("shipmentNo")]
    public string? ShipmentNo { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("pickupServices")]
    public DhlPickupServices? PickupServices { get; set; }

    [JsonPropertyName("customerReference")]
    public string? CustomerReference { get; set; }
}

internal class DhlPickupServices
{
    [JsonPropertyName("bulkyGood")]
    public bool? BulkyGood { get; set; }

    [JsonPropertyName("printLabel")]
    public bool? PrintLabel { get; set; }
}

// ──────────────────────────────────────────────────────────────
//  POST /orders — Response
// ──────────────────────────────────────────────────────────────

internal class DhlPickupOrderResponse
{
    [JsonPropertyName("confirmation")]
    public DhlPickupConfirmation? Confirmation { get; set; }
}

internal class DhlPickupConfirmation
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public DhlPickupConfirmationValue? Value { get; set; }
}

internal class DhlPickupConfirmationValue
{
    [JsonPropertyName("orderID")]
    public string? OrderID { get; set; }

    [JsonPropertyName("pickupDate")]
    public string? PickupDate { get; set; }

    [JsonPropertyName("freeOfCharge")]
    public bool? FreeOfCharge { get; set; }

    [JsonPropertyName("pickupType")]
    public string? PickupType { get; set; }

    [JsonPropertyName("confirmedShipments")]
    public DhlConfirmedShipment[]? ConfirmedShipments { get; set; }
}

internal class DhlConfirmedShipment
{
    [JsonPropertyName("transportationType")]
    public string? TransportationType { get; set; }

    [JsonPropertyName("shipmentNo")]
    public string? ShipmentNo { get; set; }

    [JsonPropertyName("orderDate")]
    public string? OrderDate { get; set; }
}

// ──────────────────────────────────────────────────────────────
//  DELETE /orders?orderID={id} — Response
// ──────────────────────────────────────────────────────────────

internal class DhlPickupCancellationResponse
{
    [JsonPropertyName("confirmedCancellations")]
    public DhlCancellationEntry[]? ConfirmedCancellations { get; set; }

    [JsonPropertyName("failedCancellations")]
    public DhlCancellationEntry[]? FailedCancellations { get; set; }
}

internal class DhlCancellationEntry
{
    [JsonPropertyName("orderID")]
    public string? OrderID { get; set; }

    [JsonPropertyName("orderState")]
    public string? OrderState { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

// ──────────────────────────────────────────────────────────────
//  GET /orders?orderID={id} — Response (array of these)
// ──────────────────────────────────────────────────────────────

internal class DhlPickupOrderStatus
{
    [JsonPropertyName("orderID")]
    public string? OrderID { get; set; }

    [JsonPropertyName("orderState")]
    public string? OrderState { get; set; }

    [JsonPropertyName("pickupDate")]
    public string? PickupDate { get; set; }

    [JsonPropertyName("pickupLocation")]
    public DhlPickupLocation? PickupLocation { get; set; }

    [JsonPropertyName("shipmentDetails")]
    public DhlShipmentDetails? ShipmentDetails { get; set; }
}

// ──────────────────────────────────────────────────────────────
//  GET /locations — Response (array of these)
// ──────────────────────────────────────────────────────────────

internal class DhlPickupLocationInfo
{
    [JsonPropertyName("asId")]
    public string? AsId { get; set; }

    [JsonPropertyName("pickupAddress")]
    public DhlPickupAddress? PickupAddress { get; set; }
}
