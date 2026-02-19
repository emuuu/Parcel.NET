namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Request for charging/topping up the Portokasse wallet (PUT /app/wallet).
/// </summary>
public class WalletChargeRequest
{
    /// <summary>
    /// Gets or sets the amount to charge in EUR cents.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the payment system. Default is "SEPA_DIRECT_DEBIT".
    /// Valid values: "SEPA_DIRECT_DEBIT", "PAYPAL".
    /// </summary>
    public string PaymentSystem { get; set; } = "SEPA_DIRECT_DEBIT";
}
