---
title: GO! Express Tracking
category: Carriers
subcategory: GO! Express
order: 2
description: Track shipments and retrieve tracking events using the GO! Express Tracking API.
apiRef: GoExpressTrackingClient
---

## Overview

The `GoExpressTrackingClient` implements the GO! Express Tracking API, providing shipment tracking by HWB (Hauptwarenbegleitschein) number.

## Basic Tracking

```csharp
var result = await trackingClient.TrackAsync("12345678");

Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Shipment: {result.ShipmentNumber}");
```

## Tracking Events

Each tracking result contains a list of events ordered by timestamp:

```csharp
foreach (var evt in result.Events)
{
    Console.WriteLine($"{evt.Timestamp:g} | {evt.Description}");
    if (evt.Location is not null)
    {
        Console.WriteLine($"  Station: {evt.Location}");
    }
}
```

## Tracking Status

The GO! Express transport status codes are mapped to the carrier-agnostic `TrackingStatus` enum:

| GO! Status | TrackingStatus | Description |
|-----------|----------------|-------------|
| `GO10` | `PreTransit` | Shipment data received |
| `GO20` | `InTransit` | Shipment in transit |
| `GO40` | `InTransit` | Shipment at destination station |
| `GO42` | `OutForDelivery` | Out for delivery |
| `GO50` | `Delivered` | Delivered |
| `GO90` | `Returned` | Returned to sender |

## Error Handling

Tracking errors throw `TrackingException`:

```csharp
try
{
    var result = await trackingClient.TrackAsync("invalid-number");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking failed: {ex.Message}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
}
```
