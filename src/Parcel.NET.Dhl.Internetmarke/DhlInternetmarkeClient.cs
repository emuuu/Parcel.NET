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
    public async Task<UserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("user", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImUserInfoResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize Internetmarke user info response.");

        return new UserInfo
        {
            DisplayName = apiResponse.DisplayName,
            Email = apiResponse.Email,
            WalletBalanceCents = apiResponse.WalletBalance ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<List<CatalogItem>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("products", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImCatalogResponse,
            cancellationToken).ConfigureAwait(false);

        return apiResponse?.Products?
            .Select(p => new CatalogItem
            {
                ProductId = p.Id ?? "",
                Name = p.Name ?? "",
                PriceCents = p.Price ?? 0,
                Type = p.Type,
                Annotation = p.Annotation,
                WeightLimitGrams = p.WeightLimit
            })
            .ToList() ?? [];
    }

    /// <inheritdoc />
    public async Task<CartResponse> InitializeCartAsync(
        CartRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = new DhlImCartRequest
        {
            Items = request.Items.Select(i => new DhlImCartItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Sender = MapAddress(i.Sender),
                Recipient = MapAddress(i.Recipient)
            }).ToList()
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "cart",
            apiRequest,
            DhlInternetmarkeJsonContext.Default.DhlImCartRequest,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImCartResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize Internetmarke cart response.");

        return new CartResponse
        {
            CartId = apiResponse.CartId ?? "",
            TotalCents = apiResponse.Total ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<CheckoutResult> CheckoutCartAsync(
        string cartId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        using var response = await _httpClient.PostAsync(
            $"cart/{Uri.EscapeDataString(cartId)}/checkout",
            null,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImCheckoutResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize Internetmarke checkout response.");

        return new CheckoutResult
        {
            OrderId = apiResponse.OrderId ?? "",
            LabelPdf = apiResponse.Label,
            TotalCents = apiResponse.Total ?? 0,
            RemainingBalanceCents = apiResponse.RemainingBalance ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<WalletBalance> GetWalletBalanceAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("wallet", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlInternetmarkeJsonContext.Default.DhlImWalletResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize Internetmarke wallet response.");

        return new WalletBalance
        {
            BalanceCents = apiResponse.Balance ?? 0
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
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
            Street = address.Street,
            PostalCode = address.PostalCode,
            City = address.City,
            CountryCode = address.CountryCode
        };
    }
}
