namespace ParcelNET.Dhl.Internetmarke.Models;

/// <summary>
/// Response from initializing an Internetmarke shopping cart.
/// </summary>
public class CartResponse
{
    /// <summary>
    /// Gets or sets the cart ID for checkout.
    /// </summary>
    public required string CartId { get; set; }

    /// <summary>
    /// Gets or sets the total price in EUR cents.
    /// </summary>
    public int TotalCents { get; set; }
}
