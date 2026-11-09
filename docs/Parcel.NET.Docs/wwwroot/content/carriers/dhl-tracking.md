---
title: DHL Tracking
category: Carriers
subcategory: DHL
order: 2
description: Track shipments with the DHL Parcel DE Tracking API v0 (XML): status, events and proof-of-delivery signature.
apiRef: DhlTrackingClient
---

## Overview

`DhlTrackingClient` (package `Parcel.NET.Dhl.Tracking`) implements the **Parcel DE Tracking API v0**, the XML interface of DHL Paket for business customers. It supports three requests:

| Method | DHL request | Purpose |
|--------|-------------|---------|
| `TrackAsync` | `d-get-piece-detail` | Status and full event history of a shipment (business customers) |
| `TrackPublicAsync` | `get-status-for-public-user` | Current status as shown on the public tracking page |
| `GetSignatureAsync` | `d-get-signature` | Proof-of-delivery signature image (GIF) |

The JSON based DHL Unified Tracking API is a separate package (`Parcel.NET.Dhl.UnifiedTracking`) and is not covered here.

## Setup and Authentication

```csharp
services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";                  // consumer key of the DHL app
    options.ApiSecret = "your-api-secret";            // consumer secret of the DHL app
    options.TrackingUsername = "your-portal-user";    // Post & DHL business customer portal user
    options.TrackingPassword = "your-portal-password";
    options.UseSandbox = true;
})
.AddDhlTracking();
```

The API needs two sets of credentials, in the sandbox and in production:

| Credential | Options | Transport |
|------------|---------|-----------|
| DHL app (API gateway) | `ApiKey` and `ApiSecret` | `Authorization: Basic base64(ApiKey:ApiSecret)` and the header `dhl-api-key` |
| Business customer portal user | `TrackingUsername` and `TrackingPassword` | `appname` and `password` attributes inside the XML query |

`DhlTrackingClient` sets the `Authorization` header on every request itself, so it also works with a manually created `HttpClient`. `AddDhlTracking()` validates on start that `ApiSecret`, `TrackingUsername` and `TrackingPassword` are set; a client created by hand throws `InvalidOperationException` when `ApiSecret` is missing.

The sandbox accepts the portal user `zt12345` with the password `geheim`. Sandbox and production use different API keys.

## Basic Tracking

```csharp
var result = await trackingClient.TrackAsync("00340434161094042557");

Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Shipment: {result.ShipmentNumber}");

if (result.EstimatedDelivery.HasValue)
{
    Console.WriteLine($"ETA: {result.EstimatedDelivery.Value:g}");
}
```

## Tracking Events

Each tracking result contains the list of events in the order returned by DHL:

```csharp
foreach (var evt in result.Events)
{
    Console.WriteLine($"{evt.Timestamp:g} | {evt.Description}");
    if (evt.Location is not null)
    {
        Console.WriteLine($"  Location: {evt.Location}");
    }
}
```

`TrackingEvent.StatusCode` is the DHL standard event code (for example `AA`, `PO`, `ZU`).

## Tracking Status

The `TrackingStatus` is derived from the current state of the shipment, using the attributes `delivery-event-flag`, `ruecksendung`, `ice` and `standard-event-code` of the shipment element (for `d-get-piece-detail` the last event is used if the shipment carries neither `ice` nor `standard-event-code`). The mapping follows the official DHL ICE/RIC/standard event code list; the first matching rule applies:

| # | Condition | Status |
|---|-----------|--------|
| 1 | `delivery-event-flag="1"` or `ice="DLVRD"` | `Delivered` |
| 2 | `ruecksendung="true"` or `ice="RETRN"` | `Returned` |
| 3 | `ice` is `DLVRF`, `NTDEL`, `DMGDS`, `DSPSD`, `EXPHD`, `LDEXC`, `MSRTD`, `MVTEX`, `NTPUP` or `CANCL`, or `standard-event-code` is `ZN`, `BV` or `AN` | `Exception` |
| 4 | `standard-event-code="ZU"` | `Delivered` |
| 5 | `standard-event-code="PO"` | `OutForDelivery` |
| 6 | `standard-event-code` is `ES`, `AE`, `AA`, `EE`, `NB`, `ZO`, `ZF`, `LA` or `GT` | `InTransit` |
| 7 | `standard-event-code` is `VA`, `DD` or `ST` | `PreTransit` |
| 8 | anything else | `Unknown` |

Note that `ZF` (delivered to a branch) and `LA` (storage) are deliberately reported as `InTransit`: the shipment is waiting for pickup but has not been handed over to the recipient. The attribute `piece-status` is not used for the status.

| Status | Description |
|--------|-------------|
| `Unknown` | Status could not be determined |
| `PreTransit` | Shipment announced, not yet in transit |
| `InTransit` | Shipment is being transported, including storage and delivery to a branch |
| `OutForDelivery` | Shipment is out for delivery |
| `Delivered` | Shipment has been delivered |
| `Exception` | A delivery exception occurred |
| `Returned` | Shipment is being returned to sender |

## Timestamps

DHL sends timestamps as `dd.MM.yyyy HH:mm` without a time zone, and does not document which zone is meant. Parcel.NET **assumes German local time (Europe/Berlin)** and returns a `DateTimeOffset` with the matching offset (+01:00 or +02:00). Parsing does not depend on the culture of the process. A time that does not exist (spring clock change) or occurs twice (autumn) is resolved to +01:00. If your data suggests a different zone, convert the values accordingly.

## DHL-Specific Options

For enhanced tracking, pass `DhlTrackingOptions`:

```csharp
var result = await trackingClient.TrackAsync("00340434161094042557", new DhlTrackingOptions
{
    Language = "en",       // "de" (default) or "en"
    ZipCode = "53113"      // recipient postal code; unlocks data classified as private
});
```

### Option Properties

| Option | Type | Description |
|--------|------|-------------|
| `Language` | `string?` | Language of the status texts: `"de"` (default) or `"en"` |
| `ZipCode` | `string?` | Recipient postal code (sender postal code for online returns). Required by DHL to receive private data such as the recipient address and the location of a branch |

## Public Tracking and Signature

```csharp
// Current status as shown on the public DHL tracking page
var publicResult = await trackingClient.TrackPublicAsync("00340434161094042557");

// Proof of delivery (GIF), only for delivered shipments within Germany
byte[]? signature = await trackingClient.GetSignatureAsync("00340434161094042557");
```

`GetSignatureAsync` returns `null` if DHL has no signature for the shipment (code `100`). DHL asks to retrieve signatures only in disputed cases or where required by law, and only once per shipment.

## Error Handling

HTTP errors and API error codes throw `TrackingException`. The DHL API reports most errors as HTTP 200 with an error `code` in the XML:

| Code | Meaning | Exception |
|------|---------|-----------|
| `100` | No data found for the tracking number | `DhlTrackingNotFoundException` |
| `5` | Login failed (wrong portal user or password) | `TrackingException` |
| `6` | Too many invalid logins | `TrackingException` |
| `62` | Missing permission | `TrackingException` |
| `200` | No electronic shipment data available | `TrackingException` |

`DhlTrackingNotFoundException` derives from `TrackingException` and exposes the `TrackingNumber`. Existing `catch (TrackingException)` blocks keep working; catch the specific type first to tell an unknown tracking number from a technical error:

```csharp
try
{
    var result = await trackingClient.TrackAsync("00340434161094099999");
}
catch (DhlTrackingNotFoundException ex)
{
    Console.WriteLine($"Unknown shipment {ex.TrackingNumber}");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking failed: {ex.Message}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
}
```

For `GetSignatureAsync`, only code `100` results in `null`; every other error code (for example `5`, or `63` if the signature archive is unavailable) throws a `TrackingException`.

## Logging and Data Protection

- The XML request, including `appname` and `password`, is part of the request URL (`?xml=...`). This is how the DHL API is designed. The default HttpClientFactory logging of Microsoft.Extensions.Http redacts the query string (`...shipments?*`), and `AddDhlTracking()` also redacts the `Authorization` and `dhl-api-key` headers. Do not add handlers, tracing or proxies that record the full request URI.
- Successful responses contain recipient data (name and address). Parcel.NET never logs them or puts them into exceptions. Error messages do not include the response body; `TrackingException.RawResponse` carries it for diagnostics, so take care when you log it.
