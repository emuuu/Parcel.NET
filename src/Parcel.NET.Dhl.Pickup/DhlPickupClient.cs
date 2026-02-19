using System.Globalization;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Pickup.Internal;
using Parcel.NET.Dhl.Pickup.Models;

namespace Parcel.NET.Dhl.Pickup;

/// <summary>
/// DHL Parcel DE Pickup API v3 client implementing <see cref="IDhlPickupClient"/>.
/// </summary>
public class DhlPickupClient : IDhlPickupClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlPickupClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Pickup API requests.</param>
    public DhlPickupClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<PickupOrderResponse> CreatePickupOrderAsync(
        PickupOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = MapToApiRequest(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "pickupOrders",
            apiRequest,
            DhlPickupJsonContext.Default.DhlPickupOrderRequest,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupOrderResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize DHL pickup order response.");

        return new PickupOrderResponse
        {
            OrderNumber = apiResponse.OrderNumber
                ?? throw new ShippingException("DHL pickup response contained no order number."),
            Status = apiResponse.Status ?? "CONFIRMED",
            ConfirmedPickupDate = apiResponse.PickupDate is not null
                ? DateTimeOffset.Parse(apiResponse.PickupDate, CultureInfo.InvariantCulture)
                : null
        };
    }

    /// <inheritdoc />
    public async Task<PickupCancellationResult> CancelPickupOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        using var response = await _httpClient.DeleteAsync(
            $"pickupOrders/{Uri.EscapeDataString(orderNumber)}",
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            return new PickupCancellationResult
            {
                Success = false,
                Message = $"Cancellation failed ({(int)response.StatusCode}): {detail}"
            };
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupCancellationResponse,
            cancellationToken).ConfigureAwait(false);

        return new PickupCancellationResult
        {
            Success = true,
            Message = apiResponse?.Message ?? "Pickup order cancelled successfully."
        };
    }

    /// <inheritdoc />
    public async Task<PickupOrderDetails> GetPickupOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        using var response = await _httpClient.GetAsync(
            $"pickupOrders/{Uri.EscapeDataString(orderNumber)}",
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupOrderDetailsResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize DHL pickup order details response.");

        return MapFromApiDetails(apiResponse);
    }

    private static DhlPickupOrderRequest MapToApiRequest(PickupOrderRequest request)
    {
        return new DhlPickupOrderRequest
        {
            CustomerDetails = new DhlPickupCustomerDetails
            {
                BillingNumber = request.BillingNumber,
                CustomerReference = request.CustomerReference
            },
            PickupDetails = new DhlPickupDetails
            {
                PickupDate = request.PickupFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ReadyByTime = request.PickupFrom.ToString("HH:mm", CultureInfo.InvariantCulture),
                ClosingTime = request.PickupUntil.ToString("HH:mm", CultureInfo.InvariantCulture),
                TotalPackages = request.PackageCount,
                TotalWeight = request.TotalWeightInKg,
                Remarks = request.Remarks
            },
            PickupAddress = new DhlPickupAddress
            {
                Name = request.Address.Name,
                Street = request.Address.Street,
                HouseNumber = request.Address.HouseNumber,
                PostalCode = request.Address.PostalCode,
                City = request.Address.City,
                Country = request.Address.Country,
                AddressAddition = request.Address.AddressAddition
            },
            ContactPerson = new DhlPickupContactPerson
            {
                Name = request.Contact.Name,
                Phone = request.Contact.Phone,
                Email = request.Contact.Email
            }
        };
    }

    private static PickupOrderDetails MapFromApiDetails(DhlPickupOrderDetailsResponse apiResponse)
    {
        var pickupDate = apiResponse.PickupDetails?.PickupDate ?? "1970-01-01";
        var readyByTime = apiResponse.PickupDetails?.ReadyByTime ?? "00:00";
        var closingTime = apiResponse.PickupDetails?.ClosingTime ?? "23:59";

        return new PickupOrderDetails
        {
            OrderNumber = apiResponse.OrderNumber
                ?? throw new ShippingException("DHL pickup details response contained no order number."),
            Status = apiResponse.Status ?? "UNKNOWN",
            Address = new PickupAddress
            {
                Name = apiResponse.PickupAddress?.Name ?? string.Empty,
                Street = apiResponse.PickupAddress?.Street ?? string.Empty,
                HouseNumber = apiResponse.PickupAddress?.HouseNumber ?? string.Empty,
                PostalCode = apiResponse.PickupAddress?.PostalCode ?? string.Empty,
                City = apiResponse.PickupAddress?.City ?? string.Empty,
                Country = apiResponse.PickupAddress?.Country ?? "DE",
                AddressAddition = apiResponse.PickupAddress?.AddressAddition
            },
            Contact = new PickupContact
            {
                Name = apiResponse.ContactPerson?.Name ?? string.Empty,
                Phone = apiResponse.ContactPerson?.Phone ?? string.Empty,
                Email = apiResponse.ContactPerson?.Email
            },
            PickupFrom = DateTimeOffset.Parse(
                $"{pickupDate}T{readyByTime}:00+00:00",
                CultureInfo.InvariantCulture),
            PickupUntil = DateTimeOffset.Parse(
                $"{pickupDate}T{closingTime}:00+00:00",
                CultureInfo.InvariantCulture),
            PackageCount = apiResponse.PickupDetails?.TotalPackages ?? 0,
            TotalWeightInKg = apiResponse.PickupDetails?.TotalWeight ?? 0,
            CustomerReference = apiResponse.CustomerDetails?.CustomerReference
        };
    }
}
