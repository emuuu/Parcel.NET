namespace ParcelNET.Dhl.Internetmarke.Models;

/// <summary>
/// User information for the Internetmarke/Portokasse account.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the current wallet balance in EUR cents.
    /// </summary>
    public int WalletBalanceCents { get; set; }
}
