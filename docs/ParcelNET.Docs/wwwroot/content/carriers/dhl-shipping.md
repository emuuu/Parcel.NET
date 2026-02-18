---
title: DHL Shipping
category: Carriers
subcategory: DHL
order: 1
description: Create shipments, validate addresses, and manage labels with the DHL Parcel DE Shipping API.
apiRef: DhlShipmentRequest
---

## Overview

The `DhlShippingClient` implements the DHL Parcel DE Shipping API v2, providing:

- Shipment creation with label generation
- Address validation
- Shipment cancellation
- Daily manifest (closing) creation

## Creating a Shipment

```csharp
var request = new DhlShipmentRequest
{
    BillingNumber = "33333333330101",
    Product = DhlProduct.V01PAK,  // Standard national parcel
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
    Packages = [new Package
    {
        Weight = 2.5,
        Dimensions = new Dimensions
        {
            Length = 30,
            Width = 20,
            Height = 15
        }
    }]
};

var response = await shippingClient.CreateShipmentAsync(request);

Console.WriteLine($"Shipment Number: {response.ShipmentNumber}");
foreach (var label in response.Labels)
{
    File.WriteAllBytes($"label.{label.Format.ToString().ToLower()}", label.Content);
}
```

## DHL Products

| Product Code | Description |
|-------------|-------------|
| `V01PAK` | DHL Paket (national, default) |
| `V53WPAK` | DHL Paket International |
| `V54EPAK` | DHL Europaket |
| `V62WP` | DHL Warenpost |
| `V66WPI` | DHL Warenpost International |

## Validating a Shipment

Validate a shipment request without actually creating it:

```csharp
var dhlClient = (IDhlShippingClient)shippingClient;
var validationResult = await dhlClient.ValidateShipmentAsync(request);

if (validationResult.Valid)
{
    Console.WriteLine("Shipment is valid!");
}
else
{
    foreach (var msg in validationResult.Messages)
    {
        Console.WriteLine($"[{msg.Severity}] {msg.Property}: {msg.Message}");
    }
}
```

## Cancelling a Shipment

```csharp
var result = await shippingClient.CancelShipmentAsync(shipmentNumber);
Console.WriteLine(result.Success ? "Cancelled" : result.Message);
```

## Creating a Manifest

Close the daily shipping manifest:

```csharp
var dhlClient = (IDhlShippingClient)shippingClient;
var manifest = await dhlClient.CreateManifestAsync();
Console.WriteLine(manifest.Success ? "Manifest created" : manifest.Message);
```

## Label Options

Configure label format and size:

```csharp
var request = new DhlShipmentRequest
{
    // ... address fields ...
    LabelOptions = new DhlLabelOptions
    {
        Format = LabelFormat.Pdf,
        PrintFormat = DhlPrintFormat.A4,
        Combine = true,
        IncludeDocs = true
    }
};
```

## Value-Added Services

```csharp
var request = new DhlShipmentRequest
{
    // ... address fields ...
    ValueAddedServices = new DhlValueAddedServices
    {
        PreferredDay = "2025-12-24",
        PreferredLocation = "Garage",
        NamedPersonOnly = true,
        AdditionalInsurance = true,
        InsuredValue = 500.00m,
        InsuredValueCurrency = "EUR"
    }
};
```

## Packstation / Post Office Delivery

Use `DhlConsignee` for alternative delivery locations:

```csharp
var request = new DhlShipmentRequest
{
    // ... shipper ...
    DhlConsignee = new DhlConsignee
    {
        Type = DhlConsigneeType.Locker,
        LockerId = "123"
    },
    Consignee = new Address
    {
        Name = "Max Mustermann",
        PostalCode = "80331",
        City = "Munich",
        CountryCode = "DEU",
        Street = "",  // Not needed for Packstation
    }
};
```
