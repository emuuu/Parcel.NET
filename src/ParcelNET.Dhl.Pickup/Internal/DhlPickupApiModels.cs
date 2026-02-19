using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Pickup.Internal;

/// <summary>
/// Internal request model for the DHL Pickup API v3 POST /pickupOrders endpoint.
/// </summary>
internal class DhlPickupOrderRequest
{
    [JsonPropertyName("customerDetails")]
    public required DhlPickupCustomerDetails CustomerDetails { get; set; }

    [JsonPropertyName("pickupDetails")]
    public required DhlPickupDetails PickupDetails { get; set; }

    [JsonPropertyName("pickupAddress")]
    public required DhlPickupAddress PickupAddress { get; set; }

    [JsonPropertyName("contactPerson")]
    public required DhlPickupContactPerson ContactPerson { get; set; }
}

internal class DhlPickupCustomerDetails
{
    [JsonPropertyName("billingNumber")]
    public string? BillingNumber { get; set; }

    [JsonPropertyName("customerReference")]
    public string? CustomerReference { get; set; }
}

internal class DhlPickupDetails
{
    [JsonPropertyName("pickupDate")]
    public required string PickupDate { get; set; }

    [JsonPropertyName("readyByTime")]
    public required string ReadyByTime { get; set; }

    [JsonPropertyName("closingTime")]
    public required string ClosingTime { get; set; }

    [JsonPropertyName("totalPackages")]
    public required int TotalPackages { get; set; }

    [JsonPropertyName("totalWeight")]
    public required double TotalWeight { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}

internal class DhlPickupAddress
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("street")]
    public required string Street { get; set; }

    [JsonPropertyName("houseNumber")]
    public required string HouseNumber { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("addressAddition")]
    public string? AddressAddition { get; set; }
}

internal class DhlPickupContactPerson
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("phone")]
    public required string Phone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// Internal response model for the DHL Pickup API v3 POST /pickupOrders endpoint.
/// </summary>
internal class DhlPickupOrderResponse
{
    [JsonPropertyName("orderNumber")]
    public string? OrderNumber { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("pickupDate")]
    public string? PickupDate { get; set; }
}

/// <summary>
/// Internal response model for the DHL Pickup API v3 GET /pickupOrders/{orderNumber} endpoint.
/// </summary>
internal class DhlPickupOrderDetailsResponse
{
    [JsonPropertyName("orderNumber")]
    public string? OrderNumber { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("pickupAddress")]
    public DhlPickupAddress? PickupAddress { get; set; }

    [JsonPropertyName("contactPerson")]
    public DhlPickupContactPerson? ContactPerson { get; set; }

    [JsonPropertyName("pickupDetails")]
    public DhlPickupDetailsResponse? PickupDetails { get; set; }

    [JsonPropertyName("customerDetails")]
    public DhlPickupCustomerDetails? CustomerDetails { get; set; }
}

internal class DhlPickupDetailsResponse
{
    [JsonPropertyName("pickupDate")]
    public string? PickupDate { get; set; }

    [JsonPropertyName("readyByTime")]
    public string? ReadyByTime { get; set; }

    [JsonPropertyName("closingTime")]
    public string? ClosingTime { get; set; }

    [JsonPropertyName("totalPackages")]
    public int TotalPackages { get; set; }

    [JsonPropertyName("totalWeight")]
    public double TotalWeight { get; set; }
}

/// <summary>
/// Internal response model for the DHL Pickup API v3 DELETE /pickupOrders/{orderNumber} endpoint.
/// </summary>
internal class DhlPickupCancellationResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
