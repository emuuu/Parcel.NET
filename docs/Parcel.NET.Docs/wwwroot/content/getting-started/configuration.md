---
title: Configuration
category: Getting Started
order: 3
description: Configure carrier API credentials, environments, and options.
---

## Overview

Each carrier in Parcel.NET has its own options class, configured via the standard .NET options pattern. You can set options inline or bind them from `appsettings.json`.

## DHL Configuration

All DHL configuration is managed through the `DhlOptions` class.

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.UseSandbox = true;
});
```

### Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `ApiKey` | `string` | **Required.** Your DHL API key. |
| `ApiSecret` | `string?` | API secret for enhanced authentication. |
| `Username` | `string?` | DHL business portal username (required for shipping). |
| `Password` | `string?` | DHL business portal password (required for shipping). |
| `UseSandbox` | `bool` | Use sandbox environment for testing. Default: `false`. |
| `CustomShippingBaseUrl` | `string?` | Override the default shipping API base URL. |
| `CustomTrackingBaseUrl` | `string?` | Override the default tracking API base URL. |

### Binding from Configuration

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

```csharp
services.AddDhl(options =>
    builder.Configuration.GetSection("Dhl").Bind(options))
    .AddDhlShipping()
    .AddDhlTracking();
```

### API Environments

DHL provides sandbox and production environments:

| Environment | Shipping Base URL | Tracking Base URL |
|-------------|-------------------|-------------------|
| Sandbox | `https://api-sandbox.dhl.com/parcel/de/shipping/v2/` | `https://api-test.dhl.com/track/shipments` |
| Production | `https://api-eu.dhl.com/parcel/de/shipping/v2/` | `https://api-eu.dhl.com/track/shipments` |

The correct base URL is selected automatically based on the `UseSandbox` setting. Use `CustomShippingBaseUrl` or `CustomTrackingBaseUrl` to override.

### Authentication

**Tracking API (API Key Only)** — The tracking API requires only an API key, which is sent via the `dhl-api-key` HTTP header.

**Shipping API (API Key + OAuth)** — The shipping API requires both an API key and OAuth authentication. Parcel.NET handles token acquisition and caching automatically using your `Username` and `Password` credentials via the Resource Owner Password Credentials (ROPC) flow.

## GO! Express Configuration

All GO! Express configuration is managed through the `GoExpressOptions` class.

```csharp
services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
    options.UseSandbox = true;
});
```

### Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `Username` | `string` | **Required.** GO! Connect API username. |
| `Password` | `string` | **Required.** GO! Connect API password. |
| `CustomerId` | `string` | **Required.** Customer ID (max 7 characters). |
| `ResponsibleStation` | `string?` | Responsible station code (3 characters). Required for shipping. |
| `UseSandbox` | `bool` | Use sandbox environment for testing. Default: `false`. |
| `CustomBaseUrl` | `string?` | Override the default API base URL. |

### Binding from Configuration

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

```csharp
services.AddGoExpress(options =>
    builder.Configuration.GetSection("GoExpress").Bind(options))
    .AddGoExpressShipping()
    .AddGoExpressTracking();
```

### API Environments

GO! Express provides sandbox and production environments:

| Environment | Shipping Base URL | Tracking Base URL |
|-------------|-------------------|-------------------|
| Sandbox | `https://ws-tst.api.general-overnight.com/external/ci/` | `https://ws-tst.api.general-overnight.com/external/api/v1/` |
| Production | `https://ws.api.general-overnight.com/external/ci/` | `https://ws.api.general-overnight.com/external/api/v1/` |

The correct base URL is selected automatically based on the `UseSandbox` setting. Use `CustomBaseUrl` to override.

### Authentication

GO! Express uses HTTP Basic Authentication. Parcel.NET encodes your `Username` and `Password` and adds the `Authorization: Basic` header to every request automatically.
