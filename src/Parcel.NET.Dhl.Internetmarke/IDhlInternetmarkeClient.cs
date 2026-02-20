using Parcel.NET.Dhl.Internetmarke.Models;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// Client interface for the DHL Post DE Internetmarke API v1.
/// </summary>
public interface IDhlInternetmarkeClient
{
    /// <summary>
    /// Retrieves the user profile data (GET /user/profile).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The user profile.</returns>
    Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves catalog data including page formats, contract products, and image catalogs (GET /app/catalog).
    /// </summary>
    /// <param name="types">Comma-separated catalog types to retrieve (e.g. "PAGE_FORMATS,CONTRACT_PRODUCTS"). If null, all types are returned.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The catalog result.</returns>
    Task<CatalogResult> GetCatalogAsync(string? types = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes an empty shopping cart (POST /app/shoppingcart).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cart response with shop order ID.</returns>
    Task<CartResponse> InitializeCartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an existing shopping cart (GET /app/shoppingcart/{shopOrderId}).
    /// </summary>
    /// <param name="shopOrderId">The shop order ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cart response.</returns>
    Task<CartResponse> GetCartAsync(string shopOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out a shopping cart as PDF, purchasing the stamps (POST /app/shoppingcart/pdf).
    /// Uses directCheckout=true by default.
    /// </summary>
    /// <param name="request">The checkout request with positions and layout options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The checkout result with download link and vouchers.</returns>
    Task<CheckoutResult> CheckoutCartPdfAsync(CheckoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out a shopping cart as PNG, purchasing the stamps (POST /app/shoppingcart/png).
    /// Uses directCheckout=true by default.
    /// </summary>
    /// <param name="request">The checkout request with positions and layout options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The checkout result with download link and vouchers.</returns>
    Task<CheckoutResult> CheckoutCartPngAsync(CheckoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Charges/tops up the Portokasse wallet (PUT /app/wallet).
    /// </summary>
    /// <param name="request">The wallet charge request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task ChargeWalletAsync(WalletChargeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a refund for purchased vouchers (POST /app/retoure).
    /// </summary>
    /// <param name="request">The retoure request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The retoure result.</returns>
    Task<RetoureResult> RequestRetoureAsync(RetoureRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the state of a retoure/refund request (GET /app/retoure).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The retoure state.</returns>
    Task<RetoureState> GetRetoureStateAsync(CancellationToken cancellationToken = default);
}
