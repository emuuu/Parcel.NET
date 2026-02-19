---
title: Dependency Injection
category: Guides
order: 2
description: Register and use Parcel.NET services with the .NET DI container.
apiRef: DhlBuilder
---

## Overview

Parcel.NET is designed for dependency injection. All services are registered via a fluent builder pattern and resolved through standard `IServiceCollection` / `IServiceProvider`.

## Basic Registration

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

## Builder Pattern

`AddDhl()` returns a `DhlBuilder` that enables chaining sub-service registrations:

```csharp
var dhlBuilder = services.AddDhl(options => { /* ... */ });

// Register individual services as needed
dhlBuilder.AddDhlShipping();   // Registers IShipmentService + IDhlShippingClient
dhlBuilder.AddDhlTracking();   // Registers ITrackingService
```

## What Gets Registered

### `AddDhl()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `DhlOptions` | Options | Configuration via IOptions pattern |
| `DhlApiKeyHandler` | Transient | HTTP handler for API key auth |
| `DhlAuthHandler` | Transient | HTTP handler for OAuth auth |
| `IDhlTokenService` | Singleton | OAuth token caching service |

### `AddDhlShipping()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `IShipmentService` | Scoped (HttpClient) | Carrier-agnostic shipping interface |
| `IDhlShippingClient` | Scoped (HttpClient) | DHL-specific shipping operations |

### `AddDhlTracking()`

| Service | Lifetime | Description |
|---------|----------|-------------|
| `ITrackingService` | Scoped (HttpClient) | Carrier-agnostic tracking interface |

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

### Using DHL-Specific Interface

For DHL-specific features like validation and manifests:

```csharp
public class DhlService(IDhlShippingClient dhlShippingClient)
{
    public async Task<ValidationResult> ValidateAsync(ShipmentRequest request)
    {
        // DHL-specific validation
        return await dhlShippingClient.ValidateShipmentAsync(request);
    }

    public async Task<ManifestResult> CloseManifestAsync()
    {
        return await dhlShippingClient.CreateManifestAsync();
    }
}
```

## Configuration via appsettings.json

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

## Selective Registration

Register only the services you need to minimize your dependency footprint:

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
