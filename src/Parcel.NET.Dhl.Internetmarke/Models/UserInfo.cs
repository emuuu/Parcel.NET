namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Authentication result from POST /user containing access token metadata and wallet balance.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets the current Portokasse wallet balance in EUR cents.
    /// </summary>
    public int WalletBalanceCents { get; set; }

    /// <summary>
    /// Gets or sets the token type (e.g. "Bearer").
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Gets or sets the token validity in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}
