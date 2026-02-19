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

        using var response = await _httpClient.PostAsJsonAsync(
            "orders",
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
            ShipmentNumber = apiResponse.ShipmentNumber ?? "",
            LabelPdf = apiResponse.LabelData,
            LabelUrl = apiResponse.LabelUrl,
            QrCode = apiResponse.QrLabelData,
            RoutingCode = apiResponse.RoutingCode
        };
    }

    /// <inheritdoc />
    public async Task<List<ReturnLocation>> GetReturnLocationsAsync(
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var url = $"locations?countryCode={Uri.EscapeDataString(countryCode)}";
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
            DhlReturnsJsonContext.Default.DhlReturnLocationResponse,
            cancellationToken).ConfigureAwait(false);

        return apiResponse?.Locations?
            .Select(l => new ReturnLocation
            {
                Id = l.Id ?? "",
                Name = l.Name ?? "",
                Street = l.Street,
                PostalCode = l.PostalCode,
                City = l.City,
                CountryCode = l.CountryCode
            })
            .ToList() ?? [];
    }

    private static DhlReturnOrderRequest MapToApiRequest(ReturnOrderRequest request)
    {
        return new DhlReturnOrderRequest
        {
            ReceiverId = request.ReceiverId,
            SenderAddress = new DhlReturnSenderAddress
            {
                Name1 = request.SenderAddress.Name,
                StreetName = request.SenderAddress.Street,
                HouseNumber = request.SenderAddress.HouseNumber,
                PostCode = request.SenderAddress.PostalCode,
                City = request.SenderAddress.City,
                Country = new DhlReturnCountry { CountryIsoCode = request.SenderAddress.CountryCode }
            },
            ShipmentReference = request.ShipmentReference,
            ReturnDocumentType = request.DocumentType switch
            {
                ReturnDocumentType.Both => "BOTH",
                ReturnDocumentType.Url => "URL",
                ReturnDocumentType.Qr => "QR",
                _ => "BOTH"
            },
            Email = request.Email,
            TelephoneNumber = request.TelephoneNumber,
            WeightInGrams = request.WeightInGrams,
            Value = request.Value
        };
    }
}
