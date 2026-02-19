using ParcelNET.Dhl.Internetmarke.Models;

namespace ParcelNET.Dhl.Internetmarke;

/// <summary>
/// Client interface for the DHL Post DE Internetmarke API v1.
/// </summary>
public interface IDhlInternetmarkeClient
{
    /// <summary>
    /// Retrieves the current user information and Portokasse balance.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The user info with wallet balance.</returns>
    Task<UserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the product catalog of available stamp types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of available catalog items.</returns>
    Task<List<CatalogItem>> GetCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes a shopping cart with the specified items.
    /// </summary>
    /// <param name="request">The cart request with items to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cart response with cart ID.</returns>
    Task<CartResponse> InitializeCartAsync(CartRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out (purchases) a shopping cart, deducting from the Portokasse balance.
    /// </summary>
    /// <param name="cartId">The cart ID to checkout.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The checkout result with labels/vouchers.</returns>
    Task<CheckoutResult> CheckoutCartAsync(string cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current Portokasse wallet balance.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The wallet balance.</returns>
    Task<WalletBalance> GetWalletBalanceAsync(CancellationToken cancellationToken = default);
}
