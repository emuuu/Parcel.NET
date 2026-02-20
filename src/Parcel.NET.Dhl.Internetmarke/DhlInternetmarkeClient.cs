using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Internetmarke.Internal;
using Parcel.NET.Dhl.Internetmarke.Models;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// DHL Post DE Internetmarke API v1 client implementing <see cref="IDhlInternetmarkeClient"/>.
/// </summary>
public class DhlInternetmarkeClient : IDhlInternetmarkeClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlInternetmarkeClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Internetmarke API requests.</param>
    public DhlInternetmarkeClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("user/profile", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImUserProfileResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke user profile response.");

        return new UserProfile
        {
            Ekp = apiResponse.Ekp,
            Company = apiResponse.Company,
            Salutation = apiResponse.Salutation,
            Title = apiResponse.Title,
            Email = apiResponse.Mail,
            Firstname = apiResponse.Firstname,
            Lastname = apiResponse.Lastname,
            Street = apiResponse.Street,
            HouseNo = apiResponse.HouseNo,
            Zip = apiResponse.Zip,
            City = apiResponse.City,
            Country = apiResponse.Country
        };
    }

    /// <inheritdoc />
    public async Task<CatalogResult> GetCatalogAsync(string? types = null, CancellationToken cancellationToken = default)
    {
        var url = types is not null
            ? $"app/catalog?types={Uri.EscapeDataString(types)}"
            : "app/catalog";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImCatalogResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke catalog response.");

        var result = new CatalogResult();

        if (apiResponse.PageFormats is not null)
        {
            result.PageFormats = apiResponse.PageFormats.Select(pf => new PageFormat
            {
                Id = pf.Id,
                Name = pf.Name,
                Description = pf.Description,
                PageType = pf.PageType,
                IsAddressPossible = pf.IsAddressPossible,
                IsImagePossible = pf.IsImagePossible
            }).ToList();
        }

        if (apiResponse.ContractProducts is not null)
        {
            result.ContractProducts = apiResponse.ContractProducts.Select(cp => new ContractProduct
            {
                ProductCode = cp.ProductCode,
                Price = cp.Price
            }).ToList();
        }

        if (apiResponse.PublicCatalog?.Items is not null)
        {
            result.PublicCatalogItems = apiResponse.PublicCatalog.Items.Select(item => new PublicCatalogItem
            {
                Category = item.Category,
                CategoryDescription = item.CategoryDescription,
                CategoryId = item.CategoryId,
                Images = item.Images ?? []
            }).ToList();
        }

        if (apiResponse.PrivateCatalog?.ImageLink is not null)
        {
            result.PrivateCatalogImageLinks = apiResponse.PrivateCatalog.ImageLink;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<CartResponse> InitializeCartAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("app/shoppingcart", null, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImShoppingCartResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke shopping cart response.");

        return new CartResponse
        {
            ShopOrderId = apiResponse.ShopOrderId
                ?? throw new ParcelException("DHL Internetmarke response missing shopOrderId.")
        };
    }

    /// <inheritdoc />
    public async Task<CartResponse> GetCartAsync(string shopOrderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shopOrderId);

        using var response = await _httpClient.GetAsync(
            $"app/shoppingcart/{Uri.EscapeDataString(shopOrderId)}", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImShoppingCartResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke shopping cart response.");

        return new CartResponse
        {
            ShopOrderId = apiResponse.ShopOrderId
                ?? throw new ParcelException("DHL Internetmarke response missing shopOrderId.")
        };
    }

    /// <inheritdoc />
    public async Task<CheckoutResult> CheckoutCartPdfAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await CheckoutCartAsync("app/shoppingcart/pdf", "AppShoppingCartPDFRequest", "AppShoppingCartPDFResponse", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CheckoutResult> CheckoutCartPngAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await CheckoutCartAsync("app/shoppingcart/png", "AppShoppingCartPNGRequest", "AppShoppingCartPNGResponse", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ChargeWalletAsync(WalletChargeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = new DhlImWalletChargeRequest
        {
            Amount = request.Amount,
            PaymentSystem = request.PaymentSystem
        };

        using var response = await _httpClient.PutAsJsonAsync(
            "app/wallet",
            apiRequest,
            DhlInternetmarkeJsonContext.Default.DhlImWalletChargeRequest,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<RetoureResult> RequestRetoureAsync(RetoureRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = new DhlImRetoureRequest
        {
            ShopRetoureId = request.ShopRetoureId,
            VoucherIds = request.VoucherIds
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "app/retoure",
            apiRequest,
            DhlInternetmarkeJsonContext.Default.DhlImRetoureRequest,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImRetoureResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke retoure response.");

        return new RetoureResult
        {
            RetoureId = apiResponse.RetoureId,
            ShopRetoureId = apiResponse.ShopRetoureId
        };
    }

    /// <inheritdoc />
    public async Task<RetoureState> GetRetoureStateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("app/retoure", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImRetoureStateResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke retoure state response.");

        return new RetoureState
        {
            RetoureId = apiResponse.RetoureId,
            ShopRetoureId = apiResponse.ShopRetoureId,
            State = apiResponse.RetoureState,
            RetoureTransactionId = apiResponse.RetoureTransactionId
        };
    }

    private async Task<CheckoutResult> CheckoutCartAsync(
        string endpoint,
        string requestType,
        string responseType,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var apiRequest = new DhlImCheckoutRequest
        {
            Type = requestType,
            ShopOrderId = request.ShopOrderId,
            Total = request.Total,
            CreateManifest = request.CreateManifest,
            CreateShippingList = request.CreateShippingList,
            Dpi = request.Dpi,
            PageFormatId = request.PageFormatId,
            Positions = request.Positions.Select(p => new DhlImCheckoutPosition
            {
                ProductCode = p.ProductCode,
                ImageId = p.ImageId,
                Address = (p.Sender is not null || p.Receiver is not null)
                    ? new DhlImCheckoutAddress
                    {
                        Sender = MapAddress(p.Sender),
                        Receiver = MapAddress(p.Receiver)
                    }
                    : null,
                VoucherLayout = p.VoucherLayout,
                Position = new DhlImLabelPosition
                {
                    LabelX = p.LabelX,
                    LabelY = p.LabelY,
                    Page = p.Page
                }
            }).ToList()
        };

        var validateParam = request.Validate ? "true" : "false";
        var url = $"{endpoint}?validate={validateParam}&directCheckout=true";

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            apiRequest,
            DhlInternetmarkeJsonContext.Default.DhlImCheckoutRequest,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImCheckoutResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize Internetmarke checkout response.");

        return new CheckoutResult
        {
            Link = apiResponse.Link,
            ManifestLink = apiResponse.ManifestLink,
            ShopOrderId = apiResponse.ShoppingCart?.ShopOrderId
                ?? request.ShopOrderId,
            Vouchers = (apiResponse.ShoppingCart?.VoucherList ?? [])
                .Select(v => new Voucher
                {
                    VoucherId = v.VoucherId,
                    TrackId = v.TrackId
                }).ToList(),
            WalletBalanceCents = apiResponse.WalletBallance ?? 0
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Internetmarke API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }
    }

    private static DhlImAddress? MapAddress(InternetmarkeAddress? address)
    {
        if (address is null) return null;
        return new DhlImAddress
        {
            Name = address.Name,
            AddressLine1 = address.AddressLine1,
            PostalCode = address.PostalCode,
            City = address.City,
            Country = address.Country
        };
    }
}
