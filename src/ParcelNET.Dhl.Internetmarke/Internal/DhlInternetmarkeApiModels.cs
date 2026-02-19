using System.Text.Json.Serialization;

namespace ParcelNET.Dhl.Internetmarke.Internal;

internal class DhlImUserInfoResponse
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("walletBalance")]
    public int? WalletBalance { get; set; }
}

internal class DhlImCatalogResponse
{
    [JsonPropertyName("products")]
    public List<DhlImCatalogProduct>? Products { get; set; }
}

internal class DhlImCatalogProduct
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public int? Price { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("annotation")]
    public string? Annotation { get; set; }

    [JsonPropertyName("weightLimit")]
    public int? WeightLimit { get; set; }
}

internal class DhlImCartRequest
{
    [JsonPropertyName("items")]
    public required List<DhlImCartItem> Items { get; set; }
}

internal class DhlImCartItem
{
    [JsonPropertyName("productId")]
    public required string ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("sender")]
    public DhlImAddress? Sender { get; set; }

    [JsonPropertyName("recipient")]
    public DhlImAddress? Recipient { get; set; }
}

internal class DhlImAddress
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

internal class DhlImCartResponse
{
    [JsonPropertyName("cartId")]
    public string? CartId { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

internal class DhlImCheckoutResponse
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("remainingBalance")]
    public int? RemainingBalance { get; set; }
}

internal class DhlImWalletResponse
{
    [JsonPropertyName("balance")]
    public int? Balance { get; set; }
}
