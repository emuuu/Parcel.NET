using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Returns.Internal;
using Parcel.NET.Dhl.Returns.Models;

namespace Parcel.NET.Dhl.Returns;

/// <summary>
/// DHL Parcel DE Returns API v1 client implementing <see cref="IDhlReturnsClient"/>.
/// </summary>
public class DhlReturnsClient : IDhlReturnsClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlReturnsClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Returns API requests.</param>
    public DhlReturnsClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<ReturnOrderResponse> CreateReturnOrderAsync(
        ReturnOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = MapToApiRequest(request);
        var labelTypeParam = MapLabelType(request.LabelType);
        var url = $"orders?labelType={Uri.EscapeDataString(labelTypeParam)}";

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            apiRequest,
            DhlReturnsJsonContext.Default.DhlReturnOrderRequest,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Returns API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlReturnsJsonContext.Default.DhlReturnOrderResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize DHL Returns response.");

        return new ReturnOrderResponse
        {
            ShipmentNo = apiResponse.ShipmentNo
                ?? throw new ParcelException("DHL Returns response missing shipment number."),
            InternationalShipmentNo = apiResponse.InternationalShipmentNo,
            LabelBase64 = apiResponse.Label?.B64,
            QrLabelBase64 = apiResponse.QrLabel?.B64,
            RoutingCode = apiResponse.RoutingCode,
            StatusTitle = apiResponse.Status?.Title,
            StatusDetail = apiResponse.Status?.Detail
        };
    }

    /// <inheritdoc />
    public async Task<List<ReturnLocation>> GetReturnLocationsAsync(
        string countryCode,
        string? receiverId = null,
        string? billingNumber = null,
        string? postalCode = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var url = $"locations?countryCode={Uri.EscapeDataString(countryCode)}";

        if (!string.IsNullOrWhiteSpace(receiverId))
            url += $"&receiverId={Uri.EscapeDataString(receiverId)}";

        if (!string.IsNullOrWhiteSpace(billingNumber))
            url += $"&billingNumber={Uri.EscapeDataString(billingNumber)}";

        if (!string.IsNullOrWhiteSpace(postalCode))
            url += $"&postalCode={Uri.EscapeDataString(postalCode)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Returns API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlReturnsJsonContext.Default.ListDhlReturnLocationItem,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize DHL Returns locations response.");

        return apiResponse
            .Select(l => new ReturnLocation
            {
                ReceiverId = l.ReceiverId
                    ?? throw new ParcelException("DHL Returns response missing location receiver ID."),
                BillingNumber = l.BillingNumber,
                Name = l.Address?.Name1,
                Street = l.Address?.AddressStreet,
                HouseNumber = l.Address?.AddressHouse,
                PostalCode = l.Address?.PostalCode,
                City = l.Address?.City,
                Country = l.Address?.Country
            })
            .ToList();
    }

    private static string MapLabelType(ReturnLabelType labelType)
    {
        return labelType switch
        {
            ReturnLabelType.ShipmentLabel => "SHIPMENT_LABEL",
            ReturnLabelType.QrLabel => "QR_LABEL",
            ReturnLabelType.Both => "BOTH",
            _ => "BOTH"
        };
    }

    private static DhlReturnOrderRequest MapToApiRequest(ReturnOrderRequest request)
    {
        var apiRequest = new DhlReturnOrderRequest
        {
            ReceiverId = request.ReceiverId,
            CustomerReference = request.CustomerReference,
            Shipper = new DhlReturnShipper
            {
                Name1 = request.Shipper.Name1,
                Name2 = request.Shipper.Name2,
                Name3 = request.Shipper.Name3,
                AddressStreet = request.Shipper.AddressStreet,
                AddressHouse = request.Shipper.AddressHouse,
                PostalCode = request.Shipper.PostalCode,
                City = request.Shipper.City,
                Country = request.Shipper.Country,
                State = request.Shipper.State,
                AdditionalAddressInformation1 = request.Shipper.AdditionalAddressInformation1,
                AdditionalAddressInformation2 = request.Shipper.AdditionalAddressInformation2,
                Email = request.Shipper.Email,
                Phone = request.Shipper.Phone
            }
        };

        if (request.WeightInGrams.HasValue)
        {
            apiRequest.ItemWeight = new DhlReturnWeight
            {
                Uom = "g",
                Value = request.WeightInGrams.Value
            };
        }

        if (request.ItemValue is not null)
        {
            apiRequest.ItemValue = new DhlReturnValue
            {
                Currency = request.ItemValue.Currency,
                Value = request.ItemValue.Value
            };
        }

        if (request.CustomsDetails is not null)
        {
            apiRequest.CustomsDetails = new DhlReturnCustomsDetails
            {
                Items = request.CustomsDetails.Items.Select(i => new DhlReturnCustomsItem
                {
                    ItemDescription = i.ItemDescription,
                    PackagedQuantity = i.PackagedQuantity,
                    ItemWeight = new DhlReturnWeight { Uom = "g", Value = i.WeightInGrams },
                    ItemValue = new DhlReturnValue { Currency = i.ItemValue.Currency, Value = i.ItemValue.Value },
                    CountryOfOrigin = i.CountryOfOrigin,
                    HsCode = i.HsCode
                }).ToList()
            };
        }

        return apiRequest;
    }
}
