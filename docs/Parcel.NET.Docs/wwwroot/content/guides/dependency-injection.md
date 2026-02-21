---
title: Dependency Injection
category: Guides
order: 2
description: Register and use Parcel.NET services with the .NET DI container.
---

## Overview

Parcel.NET is designed for dependency injection. All services are registered via a fluent builder pattern and resolved through standard `IServiceCollection` / `IServiceProvider`. Each carrier has its own builder (`DhlBuilder`, `GoExpressBuilder`) that enables chaining sub-service registrations.

## DHL Registration

### Basic Registration

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.Username = "your-username";
    options.Password = "your-password";
})
.AddDhlShipping()
.AddDhlTracking();
```

### Builder Pattern

`AddDhl()` returns a `DhlBuilder` that enables chaining sub-service registrations:

```csharp
var dhlBuilder = services.AddDhl(options => { /* ... */ });

// Register individual services as needed
dhlBuilder.AddDhlShipping();   // Registers IShipmentService + IDhlShippingClient
dhlBuilder.AddDhlTracking();   // Registers ITrackingService
```

### What Gets Registered

#### `AddDhl()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `DhlOptions` | Options | Configuration via IOptions pattern |
| `DhlApiKeyHandler` | Transient | HTTP handler for API key auth |
| `DhlAuthHandler` | Transient | HTTP handler for OAuth auth |
| `IDhlTokenService` | Singleton | OAuth token caching service |

#### `AddDhlShipping()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `IShipmentService` | Scoped (HttpClient) | Carrier-agnostic shipping interface |
| `IDhlShippingClient` | Scoped (HttpClient) | DHL-specific shipping operations |

#### `AddDhlTracking()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `ITrackingService` | Scoped (HttpClient) | Carrier-agnostic tracking interface |

## GO! Express Registration

### Basic Registration

```csharp
services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
})
.AddGoExpressShipping()
.AddGoExpressTracking();
```

### Builder Pattern

`AddGoExpress()` returns a `GoExpressBuilder` that enables chaining sub-service registrations:

```csharp
var goBuilder = services.AddGoExpress(options => { /* ... */ });

goBuilder.AddGoExpressShipping();   // Registers IShipmentService + IGoExpressShippingClient
goBuilder.AddGoExpressTracking();   // Registers ITrackingService
```

### What Gets Registered

#### `AddGoExpress()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `GoExpressOptions` | Options | Configuration via IOptions pattern |
| `GoExpressBasicAuthHandler` | Transient | HTTP handler for Basic Auth |

#### `AddGoExpressShipping()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `IShipmentService` | Transient (HttpClient) | Carrier-agnostic shipping interface |
| `IGoExpressShippingClient` | Transient (HttpClient) | GO! Express-specific shipping operations |

#### `AddGoExpressTracking()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `ITrackingService` | Transient (HttpClient) | Carrier-agnostic tracking interface |

## Injecting Services

### Using Carrier-Agnostic Interfaces

```csharp
public class OrderService(
    IShipmentService shippingClient,
    ITrackingService trackingClient)
{
    public async Task<ShipmentResponse> ShipOrderAsync(Order order)
    {
        // Works with any carrier
        return await shippingClient.CreateShipmentAsync(/* ... */);
    }

    public async Task<TrackingResult> TrackOrderAsync(string trackingNumber)
    {
        return await trackingClient.TrackAsync(trackingNumber);
    }
}
```

### Using Carrier-Specific Interfaces

For DHL-specific features like validation and manifests:

```csharp
public class DhlService(IDhlShippingClient dhlShippingClient)
{
    public async Task<ValidationResult> ValidateAsync(ShipmentRequest request)
    {
        return await dhlShippingClient.ValidateShipmentAsync(request);
    }

    public async Task<ManifestResult> CloseManifestAsync()
    {
        return await dhlShippingClient.CreateManifestAsync();
    }
}
```

For GO! Express-specific features like label generation:

```csharp
public class GoExpressService(IGoExpressShippingClient goClient)
{
    public async Task<ShipmentLabel> GetLabelAsync(string hwbNumber)
    {
        return await goClient.GenerateLabelAsync(hwbNumber, GoExpressLabelFormat.PdfA4);
    }
}
```

## Configuration via appsettings.json

### DHL

```csharp
services.AddDhl(options =>
    builder.Configuration.GetSection("Dhl").Bind(options))
    .AddDhlShipping()
    .AddDhlTracking();
```

```json
{
  "Dhl": {
    "ApiKey": "your-api-key",
    "Username": "your-username",
    "Password": "your-password",
    "UseSandbox": true
  }
}
```

### GO! Express

```csharp
services.AddGoExpress(options =>
    builder.Configuration.GetSection("GoExpress").Bind(options))
    .AddGoExpressShipping()
    .AddGoExpressTracking();
```

```json
{
  "GoExpress": {
    "Username": "your-username",
    "Password": "your-password",
    "CustomerId": "1234567",
    "ResponsibleStation": "FRA",
    "UseSandbox": true
  }
}
```

## Selective Registration

Register only the services you need to minimize your dependency footprint:

### DHL

```csharp
// Tracking only (no shipping dependencies)
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
})
.AddDhlTracking();

// Shipping only (no tracking dependencies)
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.Username = "your-username";
    options.Password = "your-password";
})
.AddDhlShipping();
```

### GO! Express

```csharp
// Tracking only
services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
})
.AddGoExpressTracking();

// Shipping only
services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
})
.AddGoExpressShipping();
```

## Multiple Carriers

You can register DHL and GO! Express side by side in the same application:

```csharp
// Register DHL
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.Username = "your-username";
    options.Password = "your-password";
})
.AddDhlShipping()
.AddDhlTracking();

// Register GO! Express
services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
})
.AddGoExpressShipping()
.AddGoExpressTracking();
```

When both carriers are registered, use carrier-specific interfaces (`IDhlShippingClient`, `IGoExpressShippingClient`) to target a particular carrier. The carrier-agnostic `IShipmentService` and `ITrackingService` will resolve to the **last registered** carrier.
