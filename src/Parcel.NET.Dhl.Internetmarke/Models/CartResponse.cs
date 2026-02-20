namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Response from initializing an Internetmarke shopping cart (POST /app/shoppingcart).
/// </summary>
public class CartResponse
{
    /// <summary>
    /// Gets or sets the shop order ID for subsequent checkout operations.
    /// </summary>
    public required string ShopOrderId { get; set; }
}
