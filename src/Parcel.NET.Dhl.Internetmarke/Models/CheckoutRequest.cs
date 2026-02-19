namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Request for checking out an Internetmarke shopping cart as PDF or PNG.
/// </summary>
public class CheckoutRequest
{
    /// <summary>
    /// Gets or sets the shop order ID from <see cref="CartResponse.ShopOrderId"/>.
    /// </summary>
    public required string ShopOrderId { get; set; }

    /// <summary>
    /// Gets or sets the total price in EUR cents.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets whether to create a manifest.
    /// </summary>
    public bool CreateManifest { get; set; }

    /// <summary>
    /// Gets or sets the shipping list option.
    /// Valid values: "NO_SHIPPING_LIST", "WITH_SHIPPING_LIST".
    /// </summary>
    public string CreateShippingList { get; set; } = "NO_SHIPPING_LIST";

    /// <summary>
    /// Gets or sets the DPI for image output. Valid values: "DPI300", "DPI600", "DPI910".
    /// Only used for PNG checkout.
    /// </summary>
    public string Dpi { get; set; } = "DPI300";

    /// <summary>
    /// Gets or sets the page format ID (from catalog).
    /// </summary>
    public int PageFormatId { get; set; }

    /// <summary>
    /// Gets or sets the label positions/items to include.
    /// </summary>
    public required List<CheckoutPosition> Positions { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the request without purchasing.
    /// </summary>
    public bool Validate { get; set; }
}

/// <summary>
/// A single position/item in a checkout request.
/// </summary>
public class CheckoutPosition
{
    /// <summary>
    /// Gets or sets the product code (e.g. 10001 for Standardbrief).
    /// </summary>
    public int ProductCode { get; set; }

    /// <summary>
    /// Gets or sets the image ID (0 for no image).
    /// </summary>
    public int? ImageId { get; set; }

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public InternetmarkeAddress? Sender { get; set; }

    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    public InternetmarkeAddress? Receiver { get; set; }

    /// <summary>
    /// Gets or sets the voucher layout.
    /// Valid values: "ADDRESS_ZONE", "FRANKING_ZONE".
    /// </summary>
    public string VoucherLayout { get; set; } = "ADDRESS_ZONE";

    /// <summary>
    /// Gets or sets the label X position on the page.
    /// </summary>
    public int LabelX { get; set; } = 1;

    /// <summary>
    /// Gets or sets the label Y position on the page.
    /// </summary>
    public int LabelY { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int Page { get; set; } = 1;
}

/// <summary>
/// Address for Internetmarke labels.
/// </summary>
public class InternetmarkeAddress
{
    /// <summary>
    /// Gets or sets the name (person or company).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets address line 1 (street and house number).
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-3 country code (e.g. "DEU").
    /// </summary>
    public string Country { get; set; } = "DEU";
}
