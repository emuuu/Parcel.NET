<p align="center">
  <img src="parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET

.NET libraries for logistics carrier APIs. Currently supports **DHL** (Shipping, Tracking, Pickup, Returns, Internetmarke, Location Finder) and **GO! Express** (Shipping, Tracking) — all targeting .NET 8, 9, and 10.

[![CI](https://github.com/emuuu/Parcel.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/emuuu/Parcel.NET/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Docs](https://img.shields.io/badge/Docs-GitHub%20Pages-blue)](https://emuuu.github.io/Parcel.NET/)

**Feature highlights:**

- Carrier-agnostic abstractions (`IShipmentService`, `ITrackingService`) for multi-carrier support
- DHL Parcel DE Shipping API v2 — create, validate, cancel shipments, manifests
- DHL Parcel DE Tracking (XML v0) and Unified Tracking (JSON) with signature retrieval
- DHL Pickup API v3 — create, cancel, and query pickup orders
- DHL Returns API v1 — create return orders, list return locations
- DHL Internetmarke/Portokasse API v1 — stamps, catalog, wallet, cart checkout
- DHL Unified Location Finder v1 — search by address, geo-coordinates, or keyword
- GO! Express Shipping — create, cancel shipments, label generation
- GO! Express Tracking — track shipments
- Builder-pattern DI registration with typed `HttpClient` pipeline
- OAuth ROPC, API key, and Basic Auth — handled automatically per carrier
- Sandbox support for DHL and GO! Express

## Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| `Parcel.NET.Abstractions` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Abstractions.svg)](https://www.nuget.org/packages/Parcel.NET.Abstractions) | Shared interfaces (`IShipmentService`, `ITrackingService`) and models |
| **DHL** | | |
| `Parcel.NET.Dhl` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl) | DHL authentication, configuration, and DI extensions |
| `Parcel.NET.Dhl.Shipping` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Shipping) | DHL Parcel DE Shipping API v2 client |
| `Parcel.NET.Dhl.Tracking` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Tracking) | DHL Parcel DE Tracking XML API v0 client |
| `Parcel.NET.Dhl.UnifiedTracking` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.UnifiedTracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.UnifiedTracking) | DHL Unified Tracking JSON API client |
| `Parcel.NET.Dhl.Pickup` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Pickup.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Pickup) | DHL Parcel DE Pickup API v3 client |
| `Parcel.NET.Dhl.Returns` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Returns.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Returns) | DHL Parcel DE Returns API v1 client |
| `Parcel.NET.Dhl.Internetmarke` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Internetmarke.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Internetmarke) | DHL Post DE Internetmarke/Portokasse API v1 client |
| `Parcel.NET.Dhl.LocationFinder` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.LocationFinder.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.LocationFinder) | DHL Unified Location Finder API v1 client |
| **GO! Express** | | |
| `Parcel.NET.GoExpress` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress) | GO! Express authentication, configuration, and DI extensions |
| `Parcel.NET.GoExpress.Shipping` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Shipping) | GO! Express Shipping client |
| `Parcel.NET.GoExpress.Tracking` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Tracking) | GO! Express Tracking client |
| **Meta-Packages** | | |
| `Parcel.NET.Dhl.All` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.All.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.All) | All DHL packages |
| `Parcel.NET.GoExpress.All` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.All.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.All) | All GO! Express packages |
| `Parcel.NET.All` | [![NuGet](https://img.shields.io/nuget/v/Parcel.NET.All.svg)](https://www.nuget.org/packages/Parcel.NET.All) | All Parcel.NET packages |

## Prerequisites

- .NET 8.0, 9.0, or 10.0
- A DHL developer account and/or GO! Express credentials

## Installation

```bash
# Individual packages
dotnet add package Parcel.NET.Dhl.Shipping
dotnet add package Parcel.NET.Dhl.Tracking
dotnet add package Parcel.NET.GoExpress.Shipping

# Or install everything for a carrier
dotnet add package Parcel.NET.Dhl.All
dotnet add package Parcel.NET.GoExpress.All

# Or install all carriers at once
dotnet add package Parcel.NET.All
```

## Getting Started

### 1. Register DHL services

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.Username = "your-username";
    options.Password = "your-password";
    options.UseSandbox = true;
})
.AddDhlShipping()
.AddDhlTracking()
.AddDhlUnifiedTracking()
.AddDhlPickup()
.AddDhlReturns()
.AddDhlInternetmarke()
.AddDhlLocationFinder();
```

### 2. Register GO! Express services

```csharp
builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
    options.UseSandbox = true;
})
.AddGoExpressShipping()
.AddGoExpressTracking();
```

### 3. Create a shipment

Use the carrier-agnostic `IShipmentService` or the carrier-specific client:

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

### 4. Track a shipment

```csharp
var trackingService = serviceProvider.GetRequiredService<ITrackingService>();

var result = await trackingService.TrackAsync("00340434161094042557");
Console.WriteLine($"Status: {result.Status}");
foreach (var evt in result.Events)
{
    Console.WriteLine($"  {evt.Timestamp}: {evt.Description} ({evt.Location})");
}
```

## DHL Services

### Shipping

`IDhlShippingClient` extends the carrier-agnostic `IShipmentService` with DHL-specific operations:

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

### Tracking

Two tracking clients are available:

| Client | API | Format | Sandbox |
|--------|-----|--------|---------|
| `IDhlTrackingClient` | Parcel DE Tracking v0 | XML | Yes |
| `IDhlUnifiedTrackingClient` | Unified Tracking | JSON | No |

```csharp
// Parcel DE Tracking with signature retrieval
var trackingClient = serviceProvider.GetRequiredService<IDhlTrackingClient>();
var result = await trackingClient.TrackAsync("00340434161094042557");
var signature = await trackingClient.GetSignatureAsync("00340434161094042557");

// Unified Tracking (JSON)
var unifiedClient = serviceProvider.GetRequiredService<IDhlUnifiedTrackingClient>();
var result = await unifiedClient.TrackAsync("00340434161094042557");
```

### Pickup

```csharp
var pickupClient = serviceProvider.GetRequiredService<IDhlPickupClient>();

var order = await pickupClient.CreatePickupOrderAsync(new PickupOrderRequest { /* ... */ });
var details = await pickupClient.GetPickupOrderAsync(order.OrderNumber);
await pickupClient.CancelPickupOrderAsync(order.OrderNumber);
```

### Returns

```csharp
var returnsClient = serviceProvider.GetRequiredService<IDhlReturnsClient>();

var returnOrder = await returnsClient.CreateReturnOrderAsync(new ReturnOrderRequest { /* ... */ });
var locations = await returnsClient.GetReturnLocationsAsync("DEU");
```

### Internetmarke

```csharp
var internetmarkeClient = serviceProvider.GetRequiredService<IDhlInternetmarkeClient>();

var userInfo = await internetmarkeClient.GetUserInfoAsync();
var catalog = await internetmarkeClient.GetCatalogAsync();
var balance = await internetmarkeClient.GetWalletBalanceAsync();
var cart = await internetmarkeClient.InitializeCartAsync(new CartRequest { /* ... */ });
var checkout = await internetmarkeClient.CheckoutCartAsync(cart.CartId);
```

### Location Finder

```csharp
var locationFinder = serviceProvider.GetRequiredService<IDhlLocationFinderClient>();

var byAddress = await locationFinder.FindByAddressAsync("DEU", "Bonn", "53113");
var byGeo = await locationFinder.FindByGeoAsync(50.7374, 7.0982, 5000);
var location = await locationFinder.GetLocationByIdAsync("8003-4012221");
```

## GO! Express Services

### Shipping

```csharp
var goClient = serviceProvider.GetRequiredService<IGoExpressShippingClient>();

var response = await goClient.CreateShipmentAsync(request);
var label = await goClient.GenerateLabelAsync(response.ShipmentNumber, GoExpressLabelFormat.Pdf);
await goClient.CancelShipmentAsync(response.ShipmentNumber);
```

### Tracking

```csharp
var trackingService = serviceProvider.GetRequiredService<ITrackingService>();
var result = await trackingService.TrackAsync("GO123456789");
```

## Configuration

### DHL Options

| Property | Description | Required |
|----------|-------------|----------|
| `ApiKey` | DHL API key (OAuth client_id) | Yes |
| `ApiSecret` | DHL API secret (OAuth client_secret) | For Shipping |
| `Username` | DHL ROPC username | For Shipping |
| `Password` | DHL ROPC password | For Shipping |
| `TrackingUsername` | Parcel DE Tracking credential | For Tracking (XML) |
| `TrackingPassword` | Parcel DE Tracking credential | For Tracking (XML) |
| `InternetmarkeUsername` | Internetmarke credential | For Internetmarke |
| `InternetmarkePassword` | Internetmarke credential | For Internetmarke |
| `UseSandbox` | Use sandbox environment | No |

### GO! Express Options

| Property | Description | Required |
|----------|-------------|----------|
| `Username` | Basic Auth username | Yes |
| `Password` | Basic Auth password | Yes |
| `CustomerId` | Customer ID (max 7 chars) | Yes |
| `ResponsibleStation` | Station code (3 chars) | For Shipping |
| `UseSandbox` | Use sandbox environment | No |

## Architecture

```
Parcel.NET.Abstractions        (IShipmentService, ITrackingService, models)
    |
    +-- Parcel.NET.Dhl         (DhlOptions, DhlTokenService, auth handlers)
    |   +-- Dhl.Shipping       (IDhlShippingClient)
    |   +-- Dhl.Tracking       (IDhlTrackingClient)
    |   +-- Dhl.UnifiedTracking(IDhlUnifiedTrackingClient)
    |   +-- Dhl.Pickup         (IDhlPickupClient)
    |   +-- Dhl.Returns        (IDhlReturnsClient)
    |   +-- Dhl.Internetmarke  (IDhlInternetmarkeClient)
    |   +-- Dhl.LocationFinder (IDhlLocationFinderClient)
    |
    +-- Parcel.NET.GoExpress   (GoExpressOptions, auth handler)
        +-- GoExpress.Shipping (IGoExpressShippingClient)
        +-- GoExpress.Tracking (GoExpressTrackingClient)
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

MIT — see [LICENSE](LICENSE) for details.
