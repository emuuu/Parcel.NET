using System.Text.Json.Serialization;

namespace Parcel.NET.Dhl.Internetmarke.Internal;

// --- Authentication ---

internal class DhlImTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("wallet_balance")]
    public int? WalletBalance { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("issued_at")]
    public string? IssuedAt { get; set; }
}

// --- User Profile ---

internal class DhlImUserProfileResponse
{
    [JsonPropertyName("ekp")]
    public string? Ekp { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("salutation")]
    public string? Salutation { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("mail")]
    public string? Mail { get; set; }

    [JsonPropertyName("firstname")]
    public string? Firstname { get; set; }

    [JsonPropertyName("lastname")]
    public string? Lastname { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("houseNo")]
    public string? HouseNo { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

// --- Catalog ---

internal class DhlImCatalogResponse
{
    [JsonPropertyName("privateCatalog")]
    public DhlImPrivateCatalog? PrivateCatalog { get; set; }

    [JsonPropertyName("publicCatalog")]
    public DhlImPublicCatalog? PublicCatalog { get; set; }

    [JsonPropertyName("pageFormats")]
    public List<DhlImPageFormat>? PageFormats { get; set; }

    [JsonPropertyName("contractProducts")]
    public List<DhlImContractProduct>? ContractProducts { get; set; }
}

internal class DhlImPrivateCatalog
{
    [JsonPropertyName("imageLink")]
    public List<string>? ImageLink { get; set; }
}

internal class DhlImPublicCatalog
{
    [JsonPropertyName("items")]
    public List<DhlImPublicCatalogItem>? Items { get; set; }
}

internal class DhlImPublicCatalogItem
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("categoryDescription")]
    public string? CategoryDescription { get; set; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}

internal class DhlImPageFormat
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("pageType")]
    public string? PageType { get; set; }

    [JsonPropertyName("isAddressPossible")]
    public bool IsAddressPossible { get; set; }

    [JsonPropertyName("isImagePossible")]
    public bool IsImagePossible { get; set; }

    [JsonPropertyName("pageLayout")]
    public DhlImPageLayout? PageLayout { get; set; }
}

internal class DhlImPageLayout
{
    [JsonPropertyName("size")]
    public DhlImSize? Size { get; set; }

    [JsonPropertyName("margin")]
    public DhlImMargin? Margin { get; set; }

    [JsonPropertyName("labelSpacing")]
    public DhlImLabelSpacing? LabelSpacing { get; set; }

    [JsonPropertyName("labelCount")]
    public DhlImLabelCount? LabelCount { get; set; }
}

internal class DhlImSize
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

internal class DhlImMargin
{
    [JsonPropertyName("top")]
    public double Top { get; set; }

    [JsonPropertyName("bottom")]
    public double Bottom { get; set; }

    [JsonPropertyName("left")]
    public double Left { get; set; }

    [JsonPropertyName("right")]
    public double Right { get; set; }
}

internal class DhlImLabelSpacing
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

internal class DhlImLabelCount
{
    [JsonPropertyName("labelX")]
    public int LabelX { get; set; }

    [JsonPropertyName("labelY")]
    public int LabelY { get; set; }
}

internal class DhlImContractProduct
{
    [JsonPropertyName("productCode")]
    public int ProductCode { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }
}

// --- Shopping Cart ---

internal class DhlImShoppingCartResponse
{
    [JsonPropertyName("shopOrderId")]
    public string? ShopOrderId { get; set; }
}

// --- Checkout Request (PDF / PNG) ---

internal class DhlImCheckoutRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("shopOrderId")]
    public required string ShopOrderId { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("createManifest")]
    public bool CreateManifest { get; set; }

    [JsonPropertyName("createShippingList")]
    public string? CreateShippingList { get; set; }

    [JsonPropertyName("dpi")]
    public string? Dpi { get; set; }

    [JsonPropertyName("pageFormatId")]
    public int PageFormatId { get; set; }

    [JsonPropertyName("positions")]
    public required List<DhlImCheckoutPosition> Positions { get; set; }
}

internal class DhlImCheckoutPosition
{
    [JsonPropertyName("productCode")]
    public int ProductCode { get; set; }

    [JsonPropertyName("imageId")]
    public int? ImageId { get; set; }

    [JsonPropertyName("address")]
    public DhlImCheckoutAddress? Address { get; set; }

    [JsonPropertyName("voucherLayout")]
    public string? VoucherLayout { get; set; }

    [JsonPropertyName("position")]
    public DhlImLabelPosition? Position { get; set; }
}

internal class DhlImCheckoutAddress
{
    [JsonPropertyName("sender")]
    public DhlImAddress? Sender { get; set; }

    [JsonPropertyName("receiver")]
    public DhlImAddress? Receiver { get; set; }
}

internal class DhlImAddress
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("addressLine1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

internal class DhlImLabelPosition
{
    [JsonPropertyName("labelX")]
    public int LabelX { get; set; }

    [JsonPropertyName("labelY")]
    public int LabelY { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }
}

// --- Checkout Response (PDF / PNG) ---

internal class DhlImCheckoutResponse
{
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("manifestLink")]
    public string? ManifestLink { get; set; }

    [JsonPropertyName("shoppingCart")]
    public DhlImShoppingCartResult? ShoppingCart { get; set; }

    /// <summary>
    /// Note: The official DHL API has a typo — "walletBallance" with double L.
    /// </summary>
    [JsonPropertyName("walletBallance")]
    public int? WalletBallance { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

internal class DhlImShoppingCartResult
{
    [JsonPropertyName("shopOrderId")]
    public string? ShopOrderId { get; set; }

    [JsonPropertyName("voucherList")]
    public List<DhlImVoucher>? VoucherList { get; set; }
}

internal class DhlImVoucher
{
    [JsonPropertyName("voucherId")]
    public string? VoucherId { get; set; }

    [JsonPropertyName("trackId")]
    public string? TrackId { get; set; }
}

// --- Wallet ---

internal class DhlImWalletChargeRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("paymentSystem")]
    public required string PaymentSystem { get; set; }
}

// --- Retoure ---

internal class DhlImRetoureRequest
{
    [JsonPropertyName("shopRetoureId")]
    public string? ShopRetoureId { get; set; }

    [JsonPropertyName("voucherIds")]
    public List<string>? VoucherIds { get; set; }
}

internal class DhlImRetoureResponse
{
    [JsonPropertyName("retoureId")]
    public string? RetoureId { get; set; }

    [JsonPropertyName("shopRetoureId")]
    public string? ShopRetoureId { get; set; }
}

internal class DhlImRetoureStateResponse
{
    [JsonPropertyName("retoureId")]
    public string? RetoureId { get; set; }

    [JsonPropertyName("shopRetoureId")]
    public string? ShopRetoureId { get; set; }

    [JsonPropertyName("retoureState")]
    public string? RetoureState { get; set; }

    [JsonPropertyName("retoureTransactionId")]
    public string? RetoureTransactionId { get; set; }
}
