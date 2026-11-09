---
title: DHL Authentication
category: Carriers
subcategory: DHL
order: 3
description: Understand DHL API authentication mechanisms and token management.
---

## Authentication Overview

DHL uses different authentication schemes depending on the API:

| API | Authentication |
|-----|---------------|
| Parcel DE Tracking v0 (`Parcel.NET.Dhl.Tracking`) | API key header + Basic authentication (`ApiKey:ApiSecret`) + business customer portal user in the XML query |
| Unified Tracking, Location Finder | API key only |
| Shipping | API Key + OAuth Bearer Token |

Parcel.NET handles all authentication automatically through delegating handlers.

## API Key Authentication

The `DhlApiKeyHandler` adds the `dhl-api-key` header to every request. It is used by the Unified Tracking and Location Finder clients, which need nothing else:

```
GET /track/shipments?trackingNumber=123 HTTP/1.1
Host: api-eu.dhl.com
dhl-api-key: your-api-key
```

The API key is configured via `DhlOptions.ApiKey`.

## Parcel DE Tracking (Basic Authentication)

The Parcel DE Tracking API v0 used by `Parcel.NET.Dhl.Tracking` needs two sets of credentials, in the sandbox and in production:

1. **The DHL app** - `DhlApiKeyHandler` adds the `dhl-api-key` header, and `DhlTrackingClient` sets `Authorization: Basic base64(ApiKey:ApiSecret)` itself on every request.
2. **A business customer portal user** - `TrackingUsername` and `TrackingPassword` are sent as `appname` and `password` inside the XML query.

```
GET /parcel/de/tracking/v0/shipments?xml=... HTTP/1.1
Host: api-eu.dhl.com
Authorization: Basic base64(ApiKey:ApiSecret)
dhl-api-key: your-api-key
```

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";                  // consumer key of the DHL app
    options.ApiSecret = "your-api-secret";            // consumer secret of the DHL app
    options.TrackingUsername = "zt12345";             // sandbox portal user
    options.TrackingPassword = "geheim";              // sandbox portal password
    options.UseSandbox = true;
})
.AddDhlTracking();
```

`AddDhlTracking()` validates on start that `ApiSecret`, `TrackingUsername` and `TrackingPassword` are set. See [DHL Tracking](docs/carriers/dhl-tracking) for details.

## OAuth Authentication (Shipping)

The shipping API requires an additional OAuth Bearer token. The `DhlAuthHandler` manages the full token lifecycle:

1. **Token Acquisition** - Uses the Resource Owner Password Credentials (ROPC) flow
2. **Token Caching** - Tokens are cached in memory for their entire validity period
3. **Automatic Refresh** - Tokens are refreshed automatically before expiry

### Required Credentials

For shipping, you need:

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";         // Required for all APIs
    options.ApiSecret = "your-api-secret";   // OAuth client secret
    options.Username = "your-username";      // DHL Business Portal username
    options.Password = "your-password";      // DHL Business Portal password
})
.AddDhlShipping();
```

### Token Flow

```
1. First shipping request
2. DhlAuthHandler detects no cached token
3. DhlTokenService acquires token via ROPC
4. Token cached with expiry
5. Request sent with:
   - dhl-api-key: your-key
   - Authorization: Bearer <token>
6. Subsequent requests use cached token
7. Token refreshed before expiry
```

## Handler Architecture

Parcel.NET uses `DelegatingHandler` to inject authentication headers transparently:

```
HttpClient
  └── DhlAuthHandler (shipping) or DhlApiKeyHandler (tracking, location finder)
      └── HttpClientHandler
          └── HTTP Request
```

`DhlTrackingClient` (Parcel DE Tracking v0) additionally sets the Basic `Authorization` header on each request itself.

This architecture means:
- Authentication is completely transparent to your code
- Tokens are managed automatically
- No manual header management needed (Parcel DE Tracking sets its Basic header inside `DhlTrackingClient`)
- Thread-safe token caching via `IDhlTokenService`

## Sandbox vs Production

Both sandbox and production use the same authentication mechanism. The only difference is the API endpoints (and, for Parcel DE Tracking, separate API keys for the sandbox and production):

```csharp
options.UseSandbox = true;  // Uses sandbox endpoints
options.UseSandbox = false; // Uses production endpoints (default)
```
