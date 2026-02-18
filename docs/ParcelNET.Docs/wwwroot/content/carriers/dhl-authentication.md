---
title: DHL Authentication
category: Carriers
order: 3
description: Understand DHL API authentication mechanisms and token management.
---

## Authentication Overview

DHL uses different authentication schemes depending on the API:

| API | Authentication |
|-----|---------------|
| Tracking | API Key only |
| Shipping | API Key + OAuth Bearer Token |

ParcelNET handles all authentication automatically through delegating handlers.

## API Key Authentication

The `DhlApiKeyHandler` adds the `dhl-api-key` header to every request. This is used for the tracking API.

```
GET /track/shipments?trackingNumber=123 HTTP/1.1
Host: api-eu.dhl.com
dhl-api-key: your-api-key
```

The API key is configured via `DhlOptions.ApiKey`.

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

ParcelNET uses `DelegatingHandler` to inject authentication headers transparently:

```
HttpClient
  └── DhlAuthHandler (shipping) or DhlApiKeyHandler (tracking)
      └── HttpClientHandler
          └── HTTP Request
```

This architecture means:
- Authentication is completely transparent to your code
- Tokens are managed automatically
- No manual header management needed
- Thread-safe token caching via `IDhlTokenService`

## Sandbox vs Production

Both sandbox and production use the same authentication mechanism. The only difference is the API endpoints:

```csharp
options.UseSandbox = true;  // Uses sandbox endpoints
options.UseSandbox = false; // Uses production endpoints (default)
```
