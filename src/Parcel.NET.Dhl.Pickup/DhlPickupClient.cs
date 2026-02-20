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
        bool validateOnly = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = MapToApiRequest(request);
        var url = validateOnly ? "orders?validate=true" : "orders";

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            apiRequest,
            DhlPickupJsonContext.Default.DhlPickupOrderRequest,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupOrderResponse,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize DHL pickup order response.");

        return MapFromApiResponse(apiResponse);
    }

    /// <inheritdoc />
    public async Task<PickupCancellationResult> CancelPickupOrdersAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (orderIds.Count == 0)
            throw new ArgumentException("At least one order ID is required.", nameof(orderIds));

        var queryString = string.Join("&", orderIds.Select(id => $"orderID={Uri.EscapeDataString(id)}"));

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"orders?{queryString}");
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupCancellationResponse,
            cancellationToken).ConfigureAwait(false);

        return MapFromCancellationResponse(apiResponse);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PickupOrderDetails>> GetPickupOrdersAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (orderIds.Count == 0)
            throw new ArgumentException("At least one order ID is required.", nameof(orderIds));

        var queryString = string.Join("&", orderIds.Select(id => $"orderID={Uri.EscapeDataString(id)}"));

        using var response = await _httpClient.GetAsync(
            $"orders?{queryString}",
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupOrderStatusArray,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize DHL pickup order status response.");

        return apiResponse.Select(MapFromOrderStatus).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PickupLocationInfo>> GetPickupLocationsAsync(
        string? postalCode = null,
        CancellationToken cancellationToken = default)
    {
        var url = postalCode is not null
            ? $"locations?postalCode={Uri.EscapeDataString(postalCode)}"
            : "locations";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ParcelException(
                $"DHL Pickup API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            DhlPickupJsonContext.Default.DhlPickupLocationInfoArray,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize DHL pickup locations response.");

        return apiResponse.Select(MapFromLocationInfo).ToList();
    }

    // ──────────────────────────────────────────────────────────────
    //  Mapping: Public model → Internal API model
    // ──────────────────────────────────────────────────────────────

    private static DhlPickupOrderRequest MapToApiRequest(PickupOrderRequest request)
    {
        DhlPickupLocation location;
        if (request.Location.LocationId is not null)
        {
            location = new DhlPickupLocation
            {
                Type = "Id",
                AsId = request.Location.LocationId
            };
        }
        else if (request.Location.Address is not null)
        {
            location = new DhlPickupLocation
            {
                Type = "Address",
                PickupAddress = new DhlPickupAddress
                {
                    Name1 = request.Location.Address.Name1,
                    Name2 = request.Location.Address.Name2,
                    AddressStreet = request.Location.Address.Street,
                    AddressHouse = request.Location.Address.HouseNumber,
                    PostalCode = request.Location.Address.PostalCode,
                    City = request.Location.Address.City,
                    Country = request.Location.Address.Country,
                    State = request.Location.Address.State
                }
            };
        }
        else
        {
            throw new ArgumentException("Pickup location must have either an Address or a LocationId.");
        }

        var pickupDate = request.UseAsapScheduling
            ? new DhlPickupDate { Type = "ASAP" }
            : new DhlPickupDate
            {
                Type = "Date",
                Value = request.PickupDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

        var apiRequest = new DhlPickupOrderRequest
        {
            CustomerDetails = new DhlPickupCustomerDetails
            {
                BillingNumber = request.BillingNumber
            },
            PickupLocation = location,
            PickupDetails = new DhlPickupDetails
            {
                PickupDate = pickupDate,
                TotalWeight = request.TotalWeightInKg.HasValue
                    ? new DhlWeight { Uom = "kg", Value = request.TotalWeightInKg.Value }
                    : null,
                Comment = request.Comment
            },
            ShipmentDetails = new DhlShipmentDetails
            {
                Shipments = request.Shipments.Select(s => new DhlShipment
                {
                    TransportationType = s.TransportationType,
                    Replacement = s.Replacement,
                    ShipmentNo = s.ShipmentNo,
                    Size = s.Size,
                    PickupServices = (s.BulkyGood.HasValue || s.PrintLabel.HasValue)
                        ? new DhlPickupServices { BulkyGood = s.BulkyGood, PrintLabel = s.PrintLabel }
                        : null,
                    CustomerReference = s.CustomerReference
                }).ToArray()
            }
        };

        if (request.BusinessHours is { Count: > 0 })
        {
            apiRequest.BusinessHours = request.BusinessHours.Select(bh => new DhlTimeFrame
            {
                TimeFrom = bh.TimeFrom,
                TimeUntil = bh.TimeUntil
            }).ToArray();
        }

        if (request.ContactPersons is { Count: > 0 })
        {
            apiRequest.ContactPerson = request.ContactPersons.Select(cp => new DhlContactPerson
            {
                Name = cp.Name,
                Phone = cp.Phone,
                Email = cp.Email
            }).ToArray();
        }

        return apiRequest;
    }

    // ──────────────────────────────────────────────────────────────
    //  Mapping: Internal API model → Public model
    // ──────────────────────────────────────────────────────────────

    private static PickupOrderResponse MapFromApiResponse(DhlPickupOrderResponse apiResponse)
    {
        var confirmation = apiResponse.Confirmation
            ?? throw new ParcelException("DHL pickup response contained no confirmation.");

        var value = confirmation.Value
            ?? throw new ParcelException("DHL pickup response contained no confirmation value.");

        DateOnly? pickupDate = null;
        if (value.PickupDate is not null &&
            DateOnly.TryParseExact(value.PickupDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            pickupDate = parsed;
        }

        return new PickupOrderResponse
        {
            OrderId = value.OrderID
                ?? throw new ParcelException("DHL pickup response contained no order ID."),
            PickupDate = pickupDate,
            FreeOfCharge = value.FreeOfCharge ?? false,
            PickupType = value.PickupType,
            ConfirmationType = confirmation.Type,
            ConfirmedShipments = (value.ConfirmedShipments ?? []).Select(cs => new ConfirmedShipment
            {
                TransportationType = cs.TransportationType,
                ShipmentNo = cs.ShipmentNo,
                OrderDate = cs.OrderDate is not null
                    ? DateTimeOffset.Parse(cs.OrderDate, CultureInfo.InvariantCulture)
                    : null
            }).ToList()
        };
    }

    private static PickupCancellationResult MapFromCancellationResponse(DhlPickupCancellationResponse? apiResponse)
    {
        return new PickupCancellationResult
        {
            ConfirmedCancellations = (apiResponse?.ConfirmedCancellations ?? []).Select(e => new CancellationEntry
            {
                OrderId = e.OrderID ?? string.Empty,
                OrderState = e.OrderState,
                Message = e.Message
            }).ToList(),
            FailedCancellations = (apiResponse?.FailedCancellations ?? []).Select(e => new CancellationEntry
            {
                OrderId = e.OrderID ?? string.Empty,
                OrderState = e.OrderState,
                Message = e.Message
            }).ToList()
        };
    }

    private static PickupOrderDetails MapFromOrderStatus(DhlPickupOrderStatus status)
    {
        DateOnly? pickupDate = null;
        if (status.PickupDate is not null &&
            DateOnly.TryParseExact(status.PickupDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            pickupDate = parsed;
        }

        return new PickupOrderDetails
        {
            OrderId = status.OrderID
                ?? throw new ParcelException("DHL pickup order status contained no order ID."),
            OrderState = status.OrderState ?? "UNKNOWN",
            PickupDate = pickupDate
        };
    }

    private static PickupLocationInfo MapFromLocationInfo(DhlPickupLocationInfo info)
    {
        return new PickupLocationInfo
        {
            LocationId = info.AsId,
            Address = info.PickupAddress is not null
                ? new PickupAddress
                {
                    Name1 = info.PickupAddress.Name1,
                    Name2 = info.PickupAddress.Name2,
                    Street = info.PickupAddress.AddressStreet,
                    HouseNumber = info.PickupAddress.AddressHouse,
                    PostalCode = info.PickupAddress.PostalCode,
                    City = info.PickupAddress.City,
                    Country = info.PickupAddress.Country,
                    State = info.PickupAddress.State
                }
                : null
        };
    }
}
