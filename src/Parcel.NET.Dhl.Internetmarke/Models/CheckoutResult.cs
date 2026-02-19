namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Result of checking out an Internetmarke shopping cart (PDF or PNG).
/// </summary>
public class CheckoutResult
{
    /// <summary>
    /// Gets or sets the download link for the generated stamps (PDF or PNG).
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets the manifest download link (if createManifest was true).
    /// </summary>
    public string? ManifestLink { get; set; }

    /// <summary>
    /// Gets or sets the shop order ID.
    /// </summary>
    public required string ShopOrderId { get; set; }

    /// <summary>
    /// Gets or sets the generated vouchers with IDs and optional tracking IDs.
    /// </summary>
    public List<Voucher> Vouchers { get; set; } = [];

    /// <summary>
    /// Gets or sets the remaining wallet balance in EUR cents after checkout.
    /// Note: The official DHL API field name is "walletBallance" (typo with double L).
    /// </summary>
    public int WalletBalanceCents { get; set; }
}

/// <summary>
/// A purchased Internetmarke voucher.
/// </summary>
public class Voucher
{
    /// <summary>
    /// Gets or sets the voucher ID.
    /// </summary>
    public string? VoucherId { get; set; }

    /// <summary>
    /// Gets or sets the tracking ID (if applicable).
    /// </summary>
    public string? TrackId { get; set; }
}
