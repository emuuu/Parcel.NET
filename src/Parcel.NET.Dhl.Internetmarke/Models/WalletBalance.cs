namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// The current Portokasse wallet balance (returned from token endpoint).
/// </summary>
public class WalletBalance
{
    /// <summary>
    /// Gets or sets the current balance in EUR cents.
    /// </summary>
    public int BalanceCents { get; set; }
}
