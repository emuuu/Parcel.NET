using System.Net.Http.Json;
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Shipping.Internal;
using Parcel.NET.Dhl.Shipping.Models;

namespace Parcel.NET.Dhl.Shipping;

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

        var dhlRequest = request as DhlShipmentRequest
            ?? throw new ArgumentException("Request must be a DhlShipmentRequest.", nameof(request));

        var orderRequest = MapToOrderRequest(dhlRequest);
        var url = BuildOrderUrl(dhlRequest.LabelOptions);

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

        // profile is required for DELETE — use default profile "STANDARD_GRUPPENPROFIL"
        using var response = await _httpClient.DeleteAsync(
            $"orders?profile=STANDARD_GRUPPENPROFIL&shipment={Uri.EscapeDataString(shipmentNumber)}",
            cancellationToken).ConfigureAwait(false);

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

        return new CancellationResult
        {
            Success = true,
            Message = "Shipment cancelled successfully."
        };
    }

    /// <inheritdoc />
    public async Task<CancellationResult> CancelShipmentAsync(
        string shipmentNumber,
        string profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);

        using var response = await _httpClient.DeleteAsync(
            $"orders?profile={Uri.EscapeDataString(profile)}&shipment={Uri.EscapeDataString(shipmentNumber)}",
            cancellationToken).ConfigureAwait(false);

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

        var dhlRequest = request as DhlShipmentRequest
            ?? throw new ArgumentException("Request must be a DhlShipmentRequest.", nameof(request));

        var orderRequest = MapToOrderRequest(dhlRequest);
        var url = BuildOrderUrl(dhlRequest.LabelOptions, validate: true);

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
        => await CreateManifestAsync("STANDARD_GRUPPENPROFIL", null, null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<ManifestResult> CreateManifestAsync(
        string profile,
        List<string>? shipmentNumbers = null,
        string? billingNumber = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);

        var manifestRequest = new DhlManifestRequest
        {
            Profile = profile,
            ShipmentNumbers = shipmentNumbers,
            BillingNumber = billingNumber
        };

        using var response = await _httpClient.PostAsJsonAsync("manifests", manifestRequest, DhlShippingJsonContext.Default.DhlManifestRequest, cancellationToken).ConfigureAwait(false);

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

        return new ManifestResult
        {
            Success = true,
            Message = "Manifest created successfully."
        };
    }

    private static DhlOrderRequest MapToOrderRequest(DhlShipmentRequest request)
    {
        var package = request.Packages.FirstOrDefault();

        return new DhlOrderRequest
        {
            Profile = request.Profile,
            Shipments =
            [
                new DhlShipment
                {
                    Product = request.Product.ToString(),
                    BillingNumber = request.BillingNumber,
                    RefNo = request.Reference,
                    CostCenter = request.CostCenter,
                    CreationSoftware = request.CreationSoftware,
                    ShipDate = request.ShipDate?.ToString("yyyy-MM-dd"),
                    Shipper = MapShipper(request.Shipper, request.ShipperContact),
                    Consignee = MapConsignee(request.Consignee, request.ConsigneeContact, request.DhlConsignee),
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
                                Length = (int)Math.Round(ConvertDimension(package.Dimensions.Length, package.DimensionUnit)),
                                Width = (int)Math.Round(ConvertDimension(package.Dimensions.Width, package.DimensionUnit)),
                                Height = (int)Math.Round(ConvertDimension(package.Dimensions.Height, package.DimensionUnit))
                            }
                            : null
                    },
                    Services = MapServices(request.ValueAddedServices),
                    Customs = MapCustoms(request.CustomsDetails)
                }
            ]
        };
    }

    private static DhlApiShipper MapShipper(Address address, ContactInfo? contact) =>
        new()
        {
            Name1 = address.Name,
            Name2 = address.Name2,
            Name3 = address.Name3,
            AddressStreet = address.Street ?? throw new ArgumentException("Shipper street is required."),
            AddressHouse = address.HouseNumber,
            PostalCode = address.PostalCode,
            City = address.City,
            Country = address.CountryCode,
            Email = contact?.Email,
            ContactName = contact?.Name
        };

    private static object MapConsignee(Address address, ContactInfo? contact, DhlConsignee? dhlConsignee)
    {
        if (dhlConsignee is null or { Type: DhlConsigneeType.ContactAddress })
        {
            return new DhlApiContactAddress
            {
                Name1 = address.Name,
                Name2 = address.Name2,
                Name3 = address.Name3,
                AddressStreet = address.Street ?? throw new ArgumentException("Consignee street is required for contact address."),
                AddressHouse = address.HouseNumber,
                PostalCode = address.PostalCode,
                City = address.City,
                Country = address.CountryCode,
                ContactName = contact?.Name,
                Phone = contact?.Phone,
                Email = contact?.Email
            };
        }

        return dhlConsignee.Type switch
        {
            DhlConsigneeType.Locker => new DhlApiLocker
            {
                Name = address.Name,
                LockerID = dhlConsignee.LockerId ?? throw new ArgumentException("LockerId is required for Packstation delivery."),
                PostNumber = dhlConsignee.PostNumber ?? throw new ArgumentException("PostNumber is required for Packstation delivery."),
                PostalCode = address.PostalCode,
                City = address.City,
                Country = address.CountryCode,
                Email = contact?.Email
            },
            DhlConsigneeType.PostOffice => new DhlApiPostOffice
            {
                Name = address.Name,
                RetailID = dhlConsignee.RetailId ?? throw new ArgumentException("RetailId is required for PostOffice delivery."),
                PostNumber = dhlConsignee.PostNumber,
                PostalCode = address.PostalCode,
                City = address.City,
                Country = address.CountryCode,
                Email = contact?.Email
            },
            DhlConsigneeType.POBox => new DhlApiPOBox
            {
                Name1 = address.Name,
                PoBoxID = dhlConsignee.PoBoxId ?? throw new ArgumentException("PoBoxId is required for PO Box delivery."),
                PostalCode = address.PostalCode,
                City = address.City,
                Country = address.CountryCode,
                Email = contact?.Email
            },
            _ => throw new ArgumentException($"Unsupported consignee type: {dhlConsignee.Type}")
        };
    }

    private static DhlApiServices? MapServices(DhlValueAddedServices? vas)
    {
        if (vas is null)
            return null;

        return new DhlApiServices
        {
            PreferredDay = vas.PreferredDay,
            PreferredLocation = vas.PreferredLocation,
            PreferredNeighbour = vas.PreferredNeighbour,
            BulkyGoods = vas.BulkyGoods ? true : null,
            NamedPersonOnly = vas.NamedPersonOnly ? true : null,
            NoNeighbourDelivery = vas.NoNeighbourDelivery ? true : null,
            SignedForByRecipient = vas.SignedForByRecipient ? true : null,
            Premium = vas.Premium ? true : null,
            ClosestDropPoint = vas.ClosestDropPoint ? true : null,
            PostalDeliveryDutyPaid = vas.PostalDeliveryDutyPaid ? true : null,
            Endorsement = vas.Endorsement,
            VisualCheckOfAge = vas.VisualCheckOfAge,
            ParcelOutletRouting = vas.ParcelOutletRouting,
            AdditionalInsurance = vas.InsuredValue.HasValue
                ? new DhlApiMonetaryValue { Currency = vas.InsuredValueCurrency ?? "EUR", Value = vas.InsuredValue.Value }
                : null,
            CashOnDelivery = vas.CashOnDelivery is not null
                ? new DhlApiCashOnDelivery
                {
                    Amount = new DhlApiMonetaryValue
                    {
                        Currency = vas.CashOnDelivery.Currency,
                        Value = vas.CashOnDelivery.Amount
                    },
                    BankAccount = vas.CashOnDelivery.Iban is not null
                        ? new DhlApiBankAccount
                        {
                            AccountHolder = vas.CashOnDelivery.AccountHolder ?? "",
                            Iban = vas.CashOnDelivery.Iban,
                            Bic = vas.CashOnDelivery.Bic
                        }
                        : null,
                    AccountReference = vas.CashOnDelivery.AccountReference,
                    TransferNote1 = vas.CashOnDelivery.TransferNote1,
                    TransferNote2 = vas.CashOnDelivery.TransferNote2
                }
                : null,
            IdentCheck = vas.IdentCheck is not null
                ? new DhlApiIdentCheck
                {
                    FirstName = vas.IdentCheck.FirstName,
                    LastName = vas.IdentCheck.LastName,
                    DateOfBirth = vas.IdentCheck.DateOfBirth,
                    MinimumAge = vas.IdentCheck.MinimumAge
                }
                : null,
            DhlRetoure = vas.DhlRetoure is not null
                ? new DhlApiRetoure
                {
                    BillingNumber = vas.DhlRetoure.BillingNumber
                }
                : null
        };
    }

    private static DhlApiCustomsDetails? MapCustoms(DhlCustomsDetails? customs)
    {
        if (customs is null)
            return null;

        return new DhlApiCustomsDetails
        {
            InvoiceNo = customs.InvoiceNo,
            ExportType = customs.ExportType,
            ExportDescription = customs.ExportDescription,
            ShippingConditions = customs.ShippingConditions,
            PostalCharges = customs.PostalCharges.HasValue
                ? new DhlApiMonetaryValue { Currency = customs.PostalChargesCurrency, Value = customs.PostalCharges.Value }
                : null,
            Items = customs.Items.Select(i => new DhlApiCustomsItem
            {
                ItemDescription = i.Description,
                CountryOfOrigin = i.CountryOfOrigin,
                HsCode = i.HsCode,
                PackagedQuantity = i.Quantity,
                ItemWeight = new DhlApiWeight { Uom = "kg", Value = i.Weight },
                ItemValue = new DhlApiMonetaryValue { Currency = i.Currency, Value = i.Value }
            }).ToList()
        };
    }

    private static string BuildOrderUrl(DhlLabelOptions? options, bool validate = false)
    {
        var url = "orders";
        var queryParts = new List<string>();

        if (validate) queryParts.Add("validate=true");

        if (options is not null)
        {
            if (options.Format != LabelFormat.Pdf)
                queryParts.Add($"docFormat={MapDocFormat(options.Format)}");
            if (options.PrintFormat != DhlPrintFormat.A4)
                queryParts.Add($"printFormat={MapPrintFormat(options.PrintFormat)}");
            if (options.Combine)
                queryParts.Add("combine=true");
            if (options.IncludeDocs)
                queryParts.Add("includeDocs=true");
        }

        return queryParts.Count > 0 ? $"{url}?{string.Join("&", queryParts)}" : url;
    }

    private static string MapDocFormat(LabelFormat format) => format switch
    {
        LabelFormat.Zpl => "ZPL2",
        _ => "PDF"
    };

    private static string MapPrintFormat(DhlPrintFormat format) => format switch
    {
        DhlPrintFormat.A4 => "A4",
        DhlPrintFormat.Label_105x208 => "910-300-700",
        DhlPrintFormat.Label_105x208_oZ => "910-300-700-oZ",
        DhlPrintFormat.Label_105x148 => "910-300-300",
        DhlPrintFormat.Label_105x148_oZ => "910-300-300-oz",
        DhlPrintFormat.Label_105x209 => "910-300-710",
        DhlPrintFormat.Thermal_103x199 => "910-300-600",
        DhlPrintFormat.Thermal_103x199_V2 => "910-300-610",
        DhlPrintFormat.Thermal_103x150 => "910-300-400",
        DhlPrintFormat.Thermal_103x150_V2 => "910-300-410",
        DhlPrintFormat.Label_100x70 => "100x70mm",
        _ => "A4"
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
