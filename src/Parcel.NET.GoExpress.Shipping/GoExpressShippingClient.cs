using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.GoExpress.Internal;
using Parcel.NET.GoExpress.Shipping.Internal;
using Parcel.NET.GoExpress.Shipping.Models;

namespace Parcel.NET.GoExpress.Shipping;

/// <summary>
/// GO! Express Shipping (Realtime Order &amp; Label) API client implementing
/// <see cref="IShipmentService"/> and <see cref="IGoExpressShippingClient"/>.
/// </summary>
public class GoExpressShippingClient : IShipmentService, IGoExpressShippingClient
{
    private readonly HttpClient _httpClient;
    private readonly GoExpressOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="GoExpressShippingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for GO! Express Shipping API requests.</param>
    /// <param name="options">GO! Express configuration options.</param>
    public GoExpressShippingClient(HttpClient httpClient, IOptions<GoExpressOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ShipmentResponse> CreateShipmentAsync(
        ShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Packages is not { Count: > 0 })
            throw new ArgumentException("At least one package is required.", nameof(request));

        var goRequest = request as GoExpressShipmentRequest
            ?? throw new ArgumentException($"Request must be of type {nameof(GoExpressShipmentRequest)}.", nameof(request));

        var orderRequest = MapToOrderRequest(goRequest);

        using var response = await _httpClient.PostAsJsonAsync(
            "order/api/v1/createOrder", orderRequest,
            GoExpressShippingJsonContext.Default.GoExpressOrderRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = GoExpressErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
                $"GO! Express Shipping API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var orderResponse = await response.Content.ReadFromJsonAsync(
            GoExpressShippingJsonContext.Default.GoExpressOrderResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize GO! Express shipping response.");

        var hwbNumber = orderResponse.HwbNumber
            ?? throw new ShippingException("GO! Express response contained no HWB number.");

        var labels = new List<ShipmentLabel>();
        if (orderResponse.Label is not null)
        {
            labels.Add(new ShipmentLabel
            {
                Format = IsRawTextFormat(goRequest.LabelFormat) ? LabelFormat.Zpl : LabelFormat.Pdf,
                Content = IsRawTextFormat(goRequest.LabelFormat)
                    ? Encoding.UTF8.GetBytes(orderResponse.Label)
                    : DecodeBase64Label(orderResponse.Label)
            });
        }

        return new ShipmentResponse
        {
            ShipmentNumber = hwbNumber,
            Labels = labels
        };
    }

    /// <inheritdoc />
    public async Task<CancellationResult> CancelShipmentAsync(
        string shipmentNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentNumber);

        var statusRequest = new GoExpressUpdateStatusRequest
        {
            HwbNumber = shipmentNumber,
            OrderStatus = "Cancelled"
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "order/api/v1/updateOrderStatus", statusRequest,
            GoExpressShippingJsonContext.Default.GoExpressUpdateStatusRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = GoExpressErrorHelper.TryParseErrorDetail(rawBody);
            return new CancellationResult
            {
                Success = false,
                Message = $"Cancellation failed ({(int)response.StatusCode}): {detail}"
            };
        }

        return new CancellationResult
        {
            Success = true,
            Message = "Shipment cancelled successfully."
        };
    }

    /// <inheritdoc />
    public async Task<ShipmentLabel> GenerateLabelAsync(
        string hwbNumber,
        GoExpressLabelFormat format = GoExpressLabelFormat.PdfA4,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hwbNumber);

        var labelRequest = new GoExpressLabelRequest
        {
            HwbNumber = hwbNumber,
            LabelFormat = MapLabelFormat(format)
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "label/api/v1/generateLabelForhwb", labelRequest,
            GoExpressShippingJsonContext.Default.GoExpressLabelRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = GoExpressErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
                $"GO! Express Label API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var labelResponse = await response.Content.ReadFromJsonAsync(
            GoExpressShippingJsonContext.Default.GoExpressLabelResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize GO! Express label response.");

        var labelData = labelResponse.Label
            ?? throw new ShippingException("GO! Express label response contained no label data.");

        return new ShipmentLabel
        {
            Format = IsRawTextFormat(format) ? LabelFormat.Zpl : LabelFormat.Pdf,
            Content = IsRawTextFormat(format)
                ? Encoding.UTF8.GetBytes(labelData)
                : DecodeBase64Label(labelData)
        };
    }

    private GoExpressOrderRequest MapToOrderRequest(GoExpressShipmentRequest request)
    {
        return new GoExpressOrderRequest
        {
            ResponsibleStation = _options.ResponsibleStation,
            CustomerId = _options.CustomerId,
            Shipment = new GoExpressApiShipment
            {
                Service = request.Service.ToString(),
                Reference = request.Reference,
                Content = request.Content,
                CostCenter = request.CostCenter,
                LabelFormat = MapLabelFormat(request.LabelFormat),
                SelfPickup = MapBool(request.SelfPickup),
                SelfDelivery = MapBool(request.SelfDelivery),
                FreightCollect = MapBool(request.FreightCollect),
                IdentCheck = MapBool(request.IdentCheck),
                ReceiptNotice = MapBool(request.ReceiptNotice),
                Insurance = request.InsuranceAmount.HasValue
                    ? new GoExpressApiMoney { Amount = request.InsuranceAmount.Value.ToString("F2", CultureInfo.InvariantCulture), Currency = request.InsuranceCurrency ?? "EUR" }
                    : null,
                CashOnDelivery = request.CashOnDeliveryAmount.HasValue
                    ? new GoExpressApiMoney { Amount = request.CashOnDeliveryAmount.Value.ToString("F2", CultureInfo.InvariantCulture), Currency = request.CashOnDeliveryCurrency ?? "EUR" }
                    : null,
                ValueOfGoods = request.ValueOfGoodsAmount.HasValue
                    ? new GoExpressApiMoney { Amount = request.ValueOfGoodsAmount.Value.ToString("F2", CultureInfo.InvariantCulture), Currency = request.ValueOfGoodsCurrency ?? "EUR" }
                    : null,
                Pickup = MapTimeWindow(request.Pickup),
                Delivery = request.Delivery is not null ? MapTimeWindow(request.Delivery) : null,
                Shipper = MapAddress(request.Shipper, request.ShipperContact),
                Consignee = MapAddress(request.Consignee, request.ConsigneeContact),
                Packages = request.Packages.Select(MapPackage).ToList()
            }
        };
    }

    private static GoExpressApiAddress MapAddress(Address address, ContactInfo? contact) =>
        new()
        {
            Name = address.Name,
            Street = address.Street,
            HouseNumber = address.HouseNumber,
            PostalCode = address.PostalCode,
            City = address.City,
            CountryCode = address.CountryCode,
            State = address.State,
            ContactName = contact?.Name,
            Email = contact?.Email,
            Phone = contact?.Phone
        };

    private static GoExpressApiPackage MapPackage(Package package) =>
        new()
        {
            Weight = ConvertWeight(package.Weight, package.WeightUnit).ToString("F2", CultureInfo.InvariantCulture),
            Length = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Length, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : null,
            Width = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Width, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : null,
            Height = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Height, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : null
        };

    private static GoExpressApiTimeWindow MapTimeWindow(TimeWindow tw) =>
        new()
        {
            Date = tw.Date.ToString("yyyy-MM-dd"),
            TimeFrom = tw.TimeFrom?.ToString("HH:mm"),
            TimeTill = tw.TimeTill?.ToString("HH:mm"),
            IsWeekend = tw.IsWeekend ? "Yes" : null,
            IsHoliday = tw.IsHoliday ? "Yes" : null
        };

    internal static string MapLabelFormat(GoExpressLabelFormat format) => format switch
    {
        GoExpressLabelFormat.Zpl => "1",
        GoExpressLabelFormat.Pdf4x6 => "2",
        GoExpressLabelFormat.PdfA4 => "4",
        GoExpressLabelFormat.Tpcl => "5",
        _ => "4"
    };

    private static string? MapBool(bool value) => value ? "Yes" : null;

    private static bool IsRawTextFormat(GoExpressLabelFormat format) =>
        format is GoExpressLabelFormat.Zpl or GoExpressLabelFormat.Tpcl;

    private static byte[] DecodeBase64Label(string label)
    {
        try
        {
            return Convert.FromBase64String(label);
        }
        catch (FormatException ex)
        {
            throw new ShippingException("GO! Express returned invalid Base64 label data.", ex);
        }
    }

    internal static double ConvertWeight(double value, WeightUnit unit) => unit switch
    {
        WeightUnit.Kilogram => value,
        WeightUnit.Gram => value / 1000.0,
        WeightUnit.Pound => value * 0.45359237,
        WeightUnit.Ounce => value * 0.028349523,
        _ => value
    };

    internal static double ConvertDimension(double value, DimensionUnit unit) => unit switch
    {
        DimensionUnit.Centimeter => value,
        DimensionUnit.Millimeter => value / 10.0,
        DimensionUnit.Inch => value * 2.54,
        _ => value
    };
}
