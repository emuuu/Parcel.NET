---
title: Configuration
category: Getting Started
order: 3
description: Configure DHL API keys, OAuth, and custom base URLs.
apiRef: DhlOptions
---

## DhlOptions

All DHL configuration is managed through the `DhlOptions` class, which is bound via the standard .NET options pattern.

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.UseSandbox = true;
});
```

## Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `ApiKey` | `string` | **Required.** Your DHL API key. |
| `ApiSecret` | `string?` | API secret for enhanced authentication. |
| `Username` | `string?` | DHL business portal username (required for shipping). |
| `Password` | `string?` | DHL business portal password (required for shipping). |
| `UseSandbox` | `bool` | Use sandbox environment for testing. Default: `false`. |
| `CustomShippingBaseUrl` | `string?` | Override the default shipping API base URL. |
| `CustomTrackingBaseUrl` | `string?` | Override the default tracking API base URL. |

## Binding from Configuration

You can bind options from `appsettings.json`:

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

## API Environments

DHL provides sandbox and production environments:

| Environment | Shipping Base URL | Tracking Base URL |
|-------------|-------------------|-------------------|
| Sandbox | `https://api-sandbox.dhl.com/parcel/de/shipping/v2/` | `https://api-test.dhl.com/track/shipments` |
| Production | `https://api-eu.dhl.com/parcel/de/shipping/v2/` | `https://api-eu.dhl.com/track/shipments` |

The correct base URL is selected automatically based on the `UseSandbox` setting. Use `CustomShippingBaseUrl` or `CustomTrackingBaseUrl` to override.

## Authentication

### Tracking API (API Key Only)

The tracking API requires only an API key, which is sent via the `dhl-api-key` HTTP header.

### Shipping API (API Key + OAuth)

The shipping API requires both an API key and OAuth authentication. Parcel.NET handles token acquisition and caching automatically using your `Username` and `Password` credentials.

The `DhlAuthHandler` manages:
- Initial token acquisition via Resource Owner Password Credentials (ROPC) flow
- Automatic token caching
- Token refresh before expiry
