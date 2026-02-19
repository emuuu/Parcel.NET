using System.Net.Http.Json;
using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Internal;
using ParcelNET.Dhl.Shipping.Internal;
using ParcelNET.Dhl.Shipping.Models;

namespace ParcelNET.Dhl.Shipping;

/// <summary>
/// DHL Parcel DE Shipping API v2 client implementing <see cref="IShipmentService"/> and <see cref="IDhlShippingClient"/>.
/// </summary>
public class DhlShippingClient : IShipmentService, IDhlShippingClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="DhlShippingClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for DHL Shipping API requests.</param>
    public DhlShippingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<ShipmentResponse> CreateShipmentAsync(
        ShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Packages is not { Count: > 0 })
            throw new ArgumentException("At least one package is required.", nameof(request));

        if (request.Packages.Count != 1)
            throw new ArgumentException("DHL Shipping API currently supports exactly one package per shipment.", nameof(request));

        var dhlRequest = request as DhlShipmentRequest;
        var orderRequest = MapToOrderRequest(request);
        var url = BuildOrderUrl(dhlRequest?.LabelOptions);

        using var response = await _httpClient.PostAsJsonAsync(url, orderRequest, DhlShippingJsonContext.Default.DhlOrderRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            throw new ShippingException(
                $"DHL Shipping API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        var orderResponse = await response.Content.ReadFromJsonAsync(DhlShippingJsonContext.Default.DhlOrderResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize DHL shipping response.");

        var item = orderResponse.Items?.FirstOrDefault()
            ?? throw new ShippingException("DHL response contained no shipment items.");

        return new ShipmentResponse
        {
            ShipmentNumber = item.ShipmentNo ?? throw new ShippingException("DHL response contained no shipment number."),
            Labels = item.Label?.B64 is not null
                ? [new ShipmentLabel { Format = LabelFormat.Pdf, Content = Convert.FromBase64String(item.Label.B64) }]
                : [],
            TrackingUrl = item.ShipmentNo is not null
                ? $"https://www.dhl.de/de/privatkunden/pakete-empfangen/verfolgen.html?piececode={Uri.EscapeDataString(item.ShipmentNo)}"
                : null
        };
    }

    /// <inheritdoc />
    public async Task<CancellationResult> CancelShipmentAsync(
        string shipmentNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentNumber);

        using var response = await _httpClient.DeleteAsync($"orders?shipment={Uri.EscapeDataString(shipmentNumber)}", cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
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
    public async Task<ValidationResult> ValidateShipmentAsync(
        ShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dhlRequest = request as DhlShipmentRequest;
        var orderRequest = MapToOrderRequest(request);
        var url = BuildOrderUrl(dhlRequest?.LabelOptions, validate: true);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(orderRequest, DhlShippingJsonContext.Default.DhlOrderRequest)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var orderResponse = await response.Content.ReadFromJsonAsync(DhlShippingJsonContext.Default.DhlOrderResponse, cancellationToken).ConfigureAwait(false)
            ?? throw new ShippingException("Failed to deserialize DHL validation response.");

        var item = orderResponse.Items?.FirstOrDefault();
        var messages = item?.ValidationMessages?
            .Select(m => new ValidationMessage
            {
                Property = m.Property,
                Message = m.Message ?? "Unknown validation error",
                Severity = m.ValidationState == "Error" ? ValidationSeverity.Error : ValidationSeverity.Warning
            })
            .ToList() ?? [];

        return new ValidationResult
        {
            Valid = response.IsSuccessStatusCode && messages.All(m => m.Severity != ValidationSeverity.Error),
            Messages = messages
        };
    }

    /// <inheritdoc />
    public async Task<ManifestResult> CreateManifestAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("manifests", null, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var detail = DhlErrorHelper.TryParseErrorDetail(rawBody);
            return new ManifestResult
            {
                Success = false,
                Message = $"Manifest creation failed ({(int)response.StatusCode}): {detail}"
            };
        }

        return new ManifestResult
        {
            Success = true,
            Message = "Manifest created successfully."
        };
    }

    private static DhlOrderRequest MapToOrderRequest(ShipmentRequest request)
    {
        var dhlRequest = request as DhlShipmentRequest;
        var package = request.Packages.FirstOrDefault();

        return new DhlOrderRequest
        {
            Profile = dhlRequest?.Profile,
            Shipments =
            [
                new DhlShipment
                {
                    Product = (dhlRequest?.Product ?? DhlProduct.V01PAK).ToString(),
                    BillingNumber = dhlRequest?.BillingNumber ?? throw new ArgumentException("BillingNumber is required for DHL shipments."),
                    RefNo = request.Reference,
                    ShipDate = request.ShipDate?.ToString("yyyy-MM-dd"),
                    Shipper = MapAddress(request.Shipper, request.ShipperContact),
                    Consignee = MapConsignee(request.Consignee, request.ConsigneeContact, dhlRequest?.DhlConsignee),
                    Details = new DhlShipmentDetails
                    {
                        Weight = new DhlApiWeight
                        {
                            Uom = "kg",
                            Value = package is not null ? ConvertWeight(package.Weight, package.WeightUnit) : 0
                        },
                        Dim = package?.Dimensions is not null
                            ? new DhlApiDimensions
                            {
                                Uom = "cm",
                                Length = ConvertDimension(package.Dimensions.Length, package.DimensionUnit),
                                Width = ConvertDimension(package.Dimensions.Width, package.DimensionUnit),
                                Height = ConvertDimension(package.Dimensions.Height, package.DimensionUnit)
                            }
                            : null
                    },
                    Services = MapServices(dhlRequest?.ValueAddedServices)
                }
            ]
        };
    }

    private static DhlApiAddress MapAddress(Address address, ContactInfo? contact) =>
        new()
        {
            Name1 = address.Name,
            AddressStreet = address.Street,
            AddressHouse = address.HouseNumber,
            PostalCode = address.PostalCode,
            City = address.City,
            Country = address.CountryCode,
            Email = contact?.Email,
            Phone = contact?.Phone,
            ContactName = contact?.Name
        };

    private static DhlApiServices? MapServices(DhlValueAddedServices? vas)
    {
        if (vas is null)
        {
            return null;
        }

        return new DhlApiServices
        {
            PreferredDay = vas.PreferredDay,
            PreferredLocation = vas.PreferredLocation,
            PreferredNeighbour = vas.PreferredNeighbour,
            BulkyGoods = vas.BulkyGoods ? true : null,
            NamedPersonOnly = vas.NamedPersonOnly ? true : null,
            NoNeighbourDelivery = vas.NoNeighbourDelivery ? true : null,
            AdditionalInsurance = vas.InsuredValue.HasValue
                ? new DhlApiMonetaryValue { Currency = vas.InsuredValueCurrency ?? "EUR", Value = vas.InsuredValue.Value }
                : null,
            CashOnDelivery = vas.CashOnDeliveryAmount.HasValue
                ? new DhlApiMonetaryValue { Currency = vas.CashOnDeliveryCurrency ?? "EUR", Value = vas.CashOnDeliveryAmount.Value }
                : null
        };
    }

    private static DhlApiAddress MapConsignee(Address address, ContactInfo? contact, DhlConsignee? dhlConsignee)
    {
        var apiAddress = MapAddress(address, contact);

        if (dhlConsignee is null or { Type: DhlConsigneeType.ContactAddress })
            return apiAddress;

        switch (dhlConsignee.Type)
        {
            case DhlConsigneeType.Locker:
                apiAddress.LockerId = dhlConsignee.LockerId;
                apiAddress.AddressStreet = null;
                apiAddress.AddressHouse = null;
                break;
            case DhlConsigneeType.PostOffice:
                apiAddress.RetailId = dhlConsignee.PostOfficeId;
                break;
            case DhlConsigneeType.POBox:
                if (int.TryParse(dhlConsignee.PoBoxId, out var poBoxId))
                    apiAddress.PoBoxId = poBoxId;
                apiAddress.AddressStreet = null;
                apiAddress.AddressHouse = null;
                break;
        }

        return apiAddress;
    }

    private static string BuildOrderUrl(DhlLabelOptions? options, bool validate = false)
    {
        var url = "orders";
        var queryParts = new List<string>();

        if (validate) queryParts.Add("validate=true");

        if (options is not null)
        {
            if (options.Format != LabelFormat.Pdf)
                queryParts.Add($"labelFormat={MapLabelFormat(options.Format)}");
            if (options.PrintFormat != DhlPrintFormat.A4)
                queryParts.Add($"printFormat={options.PrintFormat}");
            if (options.Combine)
                queryParts.Add("combine=true");
            if (options.IncludeDocs)
                queryParts.Add("includeDocs=true");
        }

        return queryParts.Count > 0 ? $"{url}?{string.Join("&", queryParts)}" : url;
    }

    private static string MapLabelFormat(LabelFormat format) => format switch
    {
        LabelFormat.Zpl => "ZPL200",
        _ => "PDF"
    };

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
