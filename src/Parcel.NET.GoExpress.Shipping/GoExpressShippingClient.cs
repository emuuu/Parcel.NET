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
        if (orderResponse.HwbOrPackageLabel is not null)
        {
            labels.Add(new ShipmentLabel
            {
                Format = IsRawTextFormat(goRequest.LabelFormat) ? LabelFormat.Zpl : LabelFormat.Pdf,
                Content = IsRawTextFormat(goRequest.LabelFormat)
                    ? Encoding.UTF8.GetBytes(orderResponse.HwbOrPackageLabel)
                    : DecodeBase64Label(orderResponse.HwbOrPackageLabel)
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
            ResponsibleStation = _options.ResponsibleStation,
            CustomerId = _options.CustomerId,
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
            ResponsibleStation = _options.ResponsibleStation,
            CustomerId = _options.CustomerId,
            Hwb = hwbNumber,
            Label = MapLabelFormat(format)
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

        var labelData = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(labelData))
            throw new ShippingException("GO! Express label response contained no label data.");

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
        var totalWeightKg = request.Packages
            .Sum(p => ConvertWeight(p.Weight, p.WeightUnit));

        return new GoExpressOrderRequest
        {
            ResponsibleStation = _options.ResponsibleStation,
            CustomerId = _options.CustomerId,
            Shipment = new GoExpressApiShipment
            {
                HwbNumber = "",
                OrderStatus = "New",
                Validation = "",
                Service = request.Service.ToString(),
                Weight = totalWeightKg.ToString("F2", CultureInfo.InvariantCulture),
                PackageCount = request.Packages.Count.ToString(CultureInfo.InvariantCulture),
                Content = request.Content ?? "",
                CustomerReference = request.Reference ?? "",
                CostCenter = request.CostCenter ?? "",
                SelfPickup = MapBool(request.SelfPickup),
                SelfDelivery = MapBool(request.SelfDelivery),
                Dimensions = request.Dimensions ?? "",
                FreightCollect = MapBool(request.FreightCollect),
                IdentCheck = MapBool(request.IdentCheck),
                ReceiptNotice = MapBool(request.ReceiptNotice),
                IsNeutralPickup = MapBool(request.IsNeutralPickup),
                Pickup = MapTimeWindow(request.Pickup),
                Delivery = request.Delivery is not null ? MapTimeWindow(request.Delivery) : CreateEmptyTimeWindow(),
                Insurance = MapMoney(request.InsuranceAmount, request.InsuranceCurrency),
                ValueOfGoods = MapMoney(request.ValueOfGoodsAmount, request.ValueOfGoodsCurrency),
                CashOnDelivery = MapMoney(request.CashOnDeliveryAmount, request.CashOnDeliveryCurrency)
            },
            ConsignorAddress = MapAddress(request.Shipper, request.ShipperContact, request.ShipperRemarks, request.ShipperTelephoneAvis),
            NeutralAddress = request.NeutralAddress is not null
                ? MapAddress(request.NeutralAddress, request.NeutralAddressContact)
                : CreateEmptyAddress(),
            ConsigneeAddress = MapAddress(
                request.Consignee, request.ConsigneeContact, request.ConsigneeRemarks,
                request.ConsigneeTelephoneAvis, request.ConsigneeDeliveryCode, request.ConsigneeDeliveryCodeEncryption),
            Label = MapLabelFormat(request.LabelFormat),
            Packages = request.Packages.Select(MapPackage).ToList()
        };
    }

    private static GoExpressApiAddress CreateEmptyAddress() => new()
    {
        Name1 = "", Name2 = "", Name3 = "",
        Street = "", HouseNumber = "",
        ZipCode = "", City = "", Country = "",
        PhoneNumber = "", Email = "",
        Remarks = "", TelephoneAvis = "",
        DeliveryCode = "", DeliveryCodeEncryption = ""
    };

    private static GoExpressApiTimeWindow CreateEmptyTimeWindow() => new()
    {
        Date = "", TimeFrom = "", TimeTill = "",
        AvisFrom = "", AvisTill = "",
        WeekendOrHolidayIndicator = ""
    };

    private static GoExpressApiAddress MapAddress(
        Address address,
        ContactInfo? contact,
        string? remarks = null,
        bool telephoneAvis = false,
        string? deliveryCode = null,
        bool deliveryCodeEncryption = false) =>
        new()
        {
            Name1 = address.Name,
            Name2 = address.Name2 ?? "",
            Name3 = address.Name3 ?? "",
            Street = address.Street ?? "",
            HouseNumber = address.HouseNumber ?? "",
            ZipCode = address.PostalCode,
            City = address.City,
            Country = address.CountryCode,
            PhoneNumber = contact?.Phone ?? "",
            Email = contact?.Email ?? "",
            Remarks = remarks ?? "",
            TelephoneAvis = MapBool(telephoneAvis),
            DeliveryCode = deliveryCode ?? "",
            DeliveryCodeEncryption = MapBool(deliveryCodeEncryption)
        };

    private static GoExpressApiPackage MapPackage(Package package) =>
        new()
        {
            Length = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Length, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : "",
            Width = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Width, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : "",
            Height = package.Dimensions is not null ? ConvertDimension(package.Dimensions.Height, package.DimensionUnit).ToString("F0", CultureInfo.InvariantCulture) : ""
        };

    private static GoExpressApiTimeWindow MapTimeWindow(TimeWindow tw) =>
        new()
        {
            Date = tw.Date.ToString("dd.MM.yyyy"),
            TimeFrom = tw.TimeFrom?.ToString("HH:mm") ?? "",
            TimeTill = tw.TimeTill?.ToString("HH:mm") ?? "",
            AvisFrom = tw.AvisFrom?.ToString("HH:mm") ?? "",
            AvisTill = tw.AvisTill?.ToString("HH:mm") ?? "",
            WeekendOrHolidayIndicator = tw.WeekendOrHolidayIndicator switch
            {
                Models.WeekendOrHolidayIndicator.Saturday => "S",
                Models.WeekendOrHolidayIndicator.Holiday => "H",
                _ => ""
            }
        };

    private static GoExpressApiMoney MapMoney(decimal? amount, string? currency) =>
        new()
        {
            Amount = amount.HasValue ? amount.Value.ToString("F2", CultureInfo.InvariantCulture) : "",
            Currency = amount.HasValue ? (currency ?? "EUR") : ""
        };

    internal static string MapLabelFormat(GoExpressLabelFormat format) => format switch
    {
        GoExpressLabelFormat.Zpl => "1",
        GoExpressLabelFormat.Pdf4x6 => "2",
        GoExpressLabelFormat.PdfA4 => "4",
        GoExpressLabelFormat.Tpcl => "5",
        _ => "4"
    };

    private static string MapBool(bool value) => value ? "Yes" : "";

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
