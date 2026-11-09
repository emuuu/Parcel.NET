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
| `ApiSecret` | `string?` | API secret of the DHL app. Required for shipping (OAuth `client_secret`) and for Parcel DE Tracking (Basic authentication with `ApiKey:ApiSecret`). |
| `Username` | `string?` | DHL business portal username (required for shipping). |
| `Password` | `string?` | DHL business portal password (required for shipping). |
| `UseSandbox` | `bool` | Use sandbox environment for testing. Default: `false`. |
| `CustomShippingBaseUrl` | `string?` | Override the default shipping API base URL. |
| `TrackingUsername` | `string?` | Business customer portal user for Parcel DE Tracking (sent in the XML query). Sandbox: `zt12345`. |
| `TrackingPassword` | `string?` | Password of the portal user for Parcel DE Tracking. Sandbox: `geheim`. |
| `CustomTrackingBaseUrl` | `string?` | Override the default Parcel DE Tracking base URL. |
| `CustomUnifiedTrackingBaseUrl` | `string?` | Override the default Unified Tracking base URL. |

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

| Environment | Shipping Base URL | Tracking Base URL (Parcel DE Tracking v0, XML) |
|-------------|-------------------|-------------------|
| Sandbox | `https://api-sandbox.dhl.com/parcel/de/shipping/v2/` | `https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments` |
| Production | `https://api-eu.dhl.com/parcel/de/shipping/v2/` | `https://api-eu.dhl.com/parcel/de/tracking/v0/shipments` |

The correct base URL is selected automatically based on the `UseSandbox` setting. Use `CustomShippingBaseUrl` or `CustomTrackingBaseUrl` to override.

The separate `Parcel.NET.Dhl.UnifiedTracking` package uses the Unified Tracking API (JSON) at `https://api-eu.dhl.com/track/shipments`. It has no sandbox; override the URL with `CustomUnifiedTrackingBaseUrl`.

### Authentication

**Parcel DE Tracking (`Parcel.NET.Dhl.Tracking`, API key + Basic + portal user)** — Sandbox and production require the app credentials `ApiKey` and `ApiSecret` (sent as `Authorization: Basic base64(ApiKey:ApiSecret)` together with the `dhl-api-key` header) and a Post & DHL business customer portal user (`TrackingUsername` and `TrackingPassword`), which is sent as `appname`/`password` inside the XML query. `AddDhlTracking()` validates `ApiSecret`, `TrackingUsername` and `TrackingPassword` on start. The sandbox portal user is `zt12345` / `geheim`.

**Unified Tracking (`Parcel.NET.Dhl.UnifiedTracking`, API key only)** — The Unified Tracking API requires only the API key, which is sent via the `dhl-api-key` HTTP header.

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
