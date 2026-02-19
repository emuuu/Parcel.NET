# Parcel.NET

[![Build & Test](https://github.com/Parcel.NET/Parcel.NET/actions/workflows/build.yml/badge.svg)](https://github.com/Parcel.NET/Parcel.NET/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A .NET library for logistics carrier APIs. Currently supports **DHL** (Shipping + Tracking), designed to be extensible for additional carriers (DPD, UPS, GLS, etc.).

## Packages

| Package | Description |
|---------|-------------|
| `Parcel.NET.Abstractions` | Shared interfaces (`IShipmentService`, `ITrackingService`) and models |
| `Parcel.NET.Dhl` | DHL authentication, configuration, and DI extensions |
| `Parcel.NET.Dhl.Shipping` | DHL Parcel DE Shipping API v2 client |
| `Parcel.NET.Dhl.Tracking` | DHL Shipment Tracking API v1 client |

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Shipping
dotnet add package Parcel.NET.Dhl.Tracking
```

## Quick Start

### Service Registration

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.Username = "your-username";
    options.Password = "your-password";
    options.UseSandbox = true; // Use sandbox for testing
})
.AddDhlShipping()
.AddDhlTracking();
```

### Creating a Shipment

```csharp
var shippingService = serviceProvider.GetRequiredService<IShipmentService>();

var request = new DhlShipmentRequest
{
    BillingNumber = "33333333330101",
    Product = DhlProduct.V01PAK,
    Shipper = new Address
    {
        Name = "Max Mustermann",
        Street = "Musterstraße",
        HouseNumber = "1",
        PostalCode = "53113",
        City = "Bonn",
        CountryCode = "DEU"
    },
    Consignee = new Address
    {
        Name = "Erika Musterfrau",
        Street = "Berliner Straße",
        HouseNumber = "42",
        PostalCode = "10117",
        City = "Berlin",
        CountryCode = "DEU"
    },
    Packages = [new Package { Weight = 2.5 }]
};

var response = await shippingService.CreateShipmentAsync(request);
Console.WriteLine($"Shipment: {response.ShipmentNumber}");
```

### Tracking a Shipment

```csharp
var trackingService = serviceProvider.GetRequiredService<ITrackingService>();

var result = await trackingService.TrackAsync("00340434161094042557");
Console.WriteLine($"Status: {result.Status}");
foreach (var evt in result.Events)
{
    Console.WriteLine($"  {evt.Timestamp}: {evt.Description} ({evt.Location})");
}
```

### DHL-Specific Features

Use `IDhlShippingClient` for DHL-specific operations:

```csharp
var dhlClient = serviceProvider.GetRequiredService<IDhlShippingClient>();

// Validate before creating
var validation = await dhlClient.ValidateShipmentAsync(request);
if (validation.Valid)
{
    var response = await dhlClient.CreateShipmentAsync(request);
}

// Create daily closing manifest
var manifest = await dhlClient.CreateManifestAsync();
```

## Configuration

| Property | Description | Required |
|----------|-------------|----------|
| `ApiKey` | DHL API key | Yes |
| `ApiSecret` | DHL API secret (OAuth) | For shipping |
| `Username` | DHL ROPC username | For shipping |
| `Password` | DHL ROPC password | For shipping |
| `UseSandbox` | Use sandbox environment | No |
| `CustomShippingBaseUrl` | Override shipping API URL | No |
| `CustomTrackingBaseUrl` | Override tracking API URL | No |

## Architecture

```
Parcel.NET.Abstractions          (IShipmentService, ITrackingService, models)
    |
Parcel.NET.Dhl                   (DhlOptions, DhlTokenService, auth handlers)
    |
    +-- Parcel.NET.Dhl.Shipping  (DhlShippingClient, IDhlShippingClient)
    +-- Parcel.NET.Dhl.Tracking  (DhlTrackingClient)
```

## Error Handling

All API errors throw typed exceptions:

```csharp
try
{
    var response = await shippingService.CreateShipmentAsync(request);
}
catch (ShippingException ex)
{
    Console.WriteLine($"Status: {ex.StatusCode}, Error: {ex.ErrorCode}");
    Console.WriteLine($"Raw response: {ex.RawResponse}");
}
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for build instructions, guidelines, and how to add new carriers.

## License

MIT - see [LICENSE](LICENSE) for details.
