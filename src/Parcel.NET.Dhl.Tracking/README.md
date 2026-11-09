![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.Tracking

DHL Parcel DE Tracking XML API v0 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track shipments and retrieve delivery signatures.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Tracking)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Tracking
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.TrackingUsername = "your-tracking-username";
    options.TrackingPassword = "your-tracking-password";
})
.AddDhlTracking();

var client = serviceProvider.GetRequiredService<IDhlTrackingClient>();

// Track a shipment
var result = await client.TrackAsync("00340434161094042557");
Console.WriteLine($"Status: {result.Status}");

// Track with options
var result = await client.TrackAsync("00340434161094042557",
    new DhlTrackingOptions { Language = "de" });

// Public status (as shown on the public DHL tracking page)
var publicResult = await client.TrackPublicAsync("00340434161094042557");

// Retrieve proof-of-delivery signature (null if DHL has none)
byte[]? signature = await client.GetSignatureAsync("00340434161094042557");
```

`IDhlTrackingClient` extends `ITrackingService`. Sandbox mode is supported — set `UseSandbox = true` in `DhlOptions`.

## Authentication

Parcel DE Tracking v0 needs two sets of credentials, in the sandbox and in production:

- the DHL app: `ApiKey` and `ApiSecret` are sent as `Authorization: Basic base64(ApiKey:ApiSecret)` (set by `DhlTrackingClient` on every request) together with the `dhl-api-key` header;
- the Post & DHL business customer portal user: `TrackingUsername` and `TrackingPassword` are sent as `appname`/`password` inside the XML query. The sandbox user is `zt12345` / `geheim`.

`AddDhlTracking()` validates `ApiSecret`, `TrackingUsername` and `TrackingPassword` on start.

## Status, errors and timestamps

- `TrackingStatus` is derived from `delivery-event-flag`, `ruecksendung`, `ice` and `standard-event-code` according to the DHL ICE/RIC/standard event code list (see the documentation for the rule table). Branch delivery (`ZF`) and storage (`LA`) are reported as `InTransit`.
- An unknown tracking number (API code `100`) throws `DhlTrackingNotFoundException` (a `TrackingException` with `TrackingNumber`). All other API error codes, such as `5` (login failed), throw a plain `TrackingException`. `GetSignatureAsync` returns `null` only for code `100`.
- DHL timestamps (`dd.MM.yyyy HH:mm`) carry no time zone; they are interpreted as German local time (Europe/Berlin), which DHL does not document. Parsing is independent of the process culture.

## Logging and data protection

The XML request, including `appname` and `password`, is part of the request URL (`?xml=...`), as designed by DHL. The default HttpClientFactory logging redacts the query string (`...shipments?*`) and `AddDhlTracking()` redacts the `Authorization` and `dhl-api-key` headers; do not add handlers, tracing or proxies that record the full request URI. Successful responses contain recipient data; Parcel.NET never logs them or includes them in exception messages.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
