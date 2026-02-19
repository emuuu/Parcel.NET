namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Result of checking out an Internetmarke shopping cart.
/// </summary>
public class CheckoutResult
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the generated label/voucher as Base64-encoded PDF.
    /// </summary>
    public string? LabelPdf { get; set; }

    /// <summary>
    /// Gets or sets the total price charged in EUR cents.
    /// </summary>
    public int TotalCents { get; set; }

    /// <summary>
    /// Gets or sets the remaining wallet balance in EUR cents after checkout.
    /// </summary>
    public int RemainingBalanceCents { get; set; }
}
