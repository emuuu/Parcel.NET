---
title: GO! Express Shipping
category: Carriers
subcategory: GO! Express
order: 1
description: Create shipments, generate labels, and cancel orders with the GO! Express Realtime Order & Label API.
apiRef: GoExpressShipmentRequest
---

## Overview

The `GoExpressShippingClient` implements the GO! Express Realtime Order & Label API (GO! Connect), providing:

- Shipment creation with label generation
- Label re-generation for existing shipments
- Shipment cancellation

## Important: AX4 Web-Portal Interaction

If you also use the AX4 Web-Portal provided by Siemens, be aware of the following:

- **Delayed visibility**: Shipments created via GO! Connect are not immediately visible in the AX4 Web-Portal. They are imported as EDI orders with a delay.
- **Cancellation only via API**: Shipments created through GO! Connect can only be cancelled via the GO! Connect API, not in the AX4 Web-Portal.
- **Labels only via API**: Labels (Frachtbriefe) for GO! Connect shipments can only be retrieved through the API, not from the AX4 Web-Portal. This is a general limitation for EDI orders in AX4, similar to shipments created through third-party shipping software.
- **Tracking works in AX4**: Shipment tracking and POD (proof of delivery / signature) are available in the AX4 Web-Portal as usual.

> **Label approval required before go-live**: Before going live with a new integration, your shipping labels must be approved by GO!'s central HUB. Contact GO! Express for the approval process.

## Creating a Shipment

```csharp
var request = new GoExpressShipmentRequest
{
    Service = GoExpressService.ON,  // General Overnight
    LabelFormat = GoExpressLabelFormat.PdfA4,
    Shipper = new Address
    {
        Name = "Sender GmbH",
        Street = "Senderstrasse",
        HouseNumber = "1",
        PostalCode = "10115",
        City = "Berlin",
        CountryCode = "DEU"
    },
    Consignee = new Address
    {
        Name = "Max Mustermann",
        Street = "Empfaengerstrasse",
        HouseNumber = "42",
        PostalCode = "80331",
        City = "Munich",
        CountryCode = "DEU"
    },
    Packages = [new Package { Weight = 5.0 }],
    Content = "Electronics",
    Pickup = new TimeWindow
    {
        Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        TimeFrom = new TimeOnly(8, 0),
        TimeTill = new TimeOnly(17, 0)
    }
};

var response = await shippingClient.CreateShipmentAsync(request);

Console.WriteLine($"HWB Number: {response.ShipmentNumber}");
foreach (var label in response.Labels)
{
    File.WriteAllBytes($"label.{label.Format.ToString().ToLower()}", label.Content);
}
```

## GO! Express Services

| Service | Description |
|---------|-------------|
| `ON` | General Overnight — national overnight delivery |
| `INT` | International delivery |
| `LET` | Letter Express — national document delivery |
| `INL` | International Letter Express |
| `PSN` | Parcel Service National |
| `PSI` | Parcel Service International |
| `ONC` | Overnight City — same-city overnight |
| `LEC` | Letter Express City — same-city letter |
| `DI` | Direct / Individual — custom direct transport |

## Label Formats

| Format | Description |
|--------|-------------|
| `PdfA4` | PDF A4 (default) |
| `Pdf4x6` | PDF 4×6 inch |
| `Zpl` | ZPL (Zebra printer language) |
| `Tpcl` | TPCL (Toshiba printer language) |

## Time Windows

Pickup is required; delivery is optional:

```csharp
var request = new GoExpressShipmentRequest
{
    // ... addresses, packages ...
    Pickup = new TimeWindow
    {
        Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        TimeFrom = new TimeOnly(8, 0),
        TimeTill = new TimeOnly(17, 0),
        AvisFrom = new TimeOnly(7, 30),            // Notification window
        AvisTill = new TimeOnly(8, 0),
        WeekendOrHolidayIndicator = WeekendOrHolidayIndicator.Saturday
    },
    Delivery = new TimeWindow
    {
        Date = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
        TimeFrom = new TimeOnly(9, 0),
        TimeTill = new TimeOnly(12, 0)
    }
};
```

## Value-Added Services

```csharp
var request = new GoExpressShipmentRequest
{
    // ... addresses, packages, pickup ...
    SelfPickup = true,          // Sender delivers to GO! station
    SelfDelivery = false,       // Recipient picks up at GO! station
    FreightCollect = false,     // Recipient pays freight
    IdentCheck = true,          // Identity verification at delivery
    ReceiptNotice = true,       // Receipt notice required
    IsNeutralPickup = true,     // Hide consignor address from consignee
    NeutralAddress = new Address { /* ... */ },
    InsuranceAmount = 1000m,
    InsuranceCurrency = "EUR",
    CashOnDeliveryAmount = 50m,
    CashOnDeliveryCurrency = "EUR",
    ValueOfGoodsAmount = 500m,
    ValueOfGoodsCurrency = "EUR",
    CostCenter = "CC-1234",
    Dimensions = "120x80x60",
    ShipperRemarks = "Gate code: 4321",
    ShipperTelephoneAvis = true,
    ConsigneeRemarks = "Leave at door",
    ConsigneeTelephoneAvis = true,
    ConsigneeDeliveryCode = "SECRET",
    ConsigneeDeliveryCodeEncryption = true
};
```

## Generating a Label

Re-generate a label for an existing shipment:

```csharp
var goClient = (IGoExpressShippingClient)shippingClient;
var label = await goClient.GenerateLabelAsync("12345678", GoExpressLabelFormat.PdfA4);
File.WriteAllBytes("label.pdf", label.Content);
```

## Cancelling a Shipment

```csharp
var result = await shippingClient.CancelShipmentAsync(hwbNumber);
Console.WriteLine(result.Success ? "Cancelled" : result.Message);
```

## Error Handling

Shipping errors throw `ShippingException`:

```csharp
try
{
    var response = await shippingClient.CreateShipmentAsync(request);
}
catch (ShippingException ex)
{
    Console.WriteLine($"Shipping failed: {ex.Message}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Raw Response: {ex.RawResponse}");
}
```
