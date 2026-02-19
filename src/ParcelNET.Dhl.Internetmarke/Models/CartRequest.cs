namespace ParcelNET.Dhl.Internetmarke.Models;

/// <summary>
/// Request to initialize an Internetmarke shopping cart.
/// </summary>
public class CartRequest
{
    /// <summary>
    /// Gets or sets the items to add to the cart.
    /// </summary>
    public required List<CartItem> Items { get; set; }
}

/// <summary>
/// A single item in an Internetmarke cart.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Gets or sets the product ID (PPL-ID).
    /// </summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the sender address (optional).
    /// </summary>
    public InternetmarkeAddress? Sender { get; set; }

    /// <summary>
    /// Gets or sets the recipient address (optional).
    /// </summary>
    public InternetmarkeAddress? Recipient { get; set; }
}

/// <summary>
/// Address for Internetmarke labels.
/// </summary>
public class InternetmarkeAddress
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the street with house number.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO country code.
    /// </summary>
    public string CountryCode { get; set; } = "DEU";
}
