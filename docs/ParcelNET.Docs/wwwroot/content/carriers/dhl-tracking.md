---
title: DHL Tracking
category: Carriers
subcategory: DHL
order: 2
description: Track shipments and retrieve tracking events using the DHL Tracking API.
apiRef: DhlTrackingClient
---

## Overview

The `DhlTrackingClient` implements the DHL Shipment Tracking API v1, providing real-time shipment tracking with event history.

## Basic Tracking

```csharp
var result = await trackingClient.TrackAsync("1234567890");

Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Shipment: {result.ShipmentNumber}");

if (result.EstimatedDelivery.HasValue)
{
    Console.WriteLine($"ETA: {result.EstimatedDelivery.Value:g}");
}
```

## Tracking Events

Each tracking result contains a list of events ordered by timestamp:

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

## Tracking Status

The `TrackingStatus` enum provides a carrier-agnostic status:

| Status | Description |
|--------|-------------|
| `Unknown` | Status could not be determined |
| `PreTransit` | Shipment information received, not yet in transit |
| `InTransit` | Shipment is being transported |
| `OutForDelivery` | Shipment is out for delivery |
| `Delivered` | Shipment has been delivered |
| `Exception` | A delivery exception occurred |
| `Returned` | Shipment is being returned to sender |

## DHL-Specific Options

For enhanced tracking, pass `DhlTrackingOptions`:

```csharp
var dhlClient = (DhlTrackingClient)trackingClient;

var result = await dhlClient.TrackAsync("1234567890", new DhlTrackingOptions
{
    Language = "de",                    // German event descriptions
    RecipientPostalCode = "80331",      // Enhanced tracking data
    OriginCountryCode = "DE"            // Cross-border filter
});
```

### Option Properties

| Option | Type | Description |
|--------|------|-------------|
| `Language` | `string?` | Preferred language for event descriptions (e.g., `"en"`, `"de"`) |
| `RecipientPostalCode` | `string?` | Recipient postal code for enhanced tracking data |
| `OriginCountryCode` | `string?` | Origin country code for cross-border shipment filtering |

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
