---
title: Quick Start
category: Getting Started
order: 2
description: Get up and running with ParcelNET in minutes.
---

## Minimal Example

This example demonstrates registering DHL services, creating a shipment, and tracking it.

### 1. Register Services

```csharp
using ParcelNET.Dhl;
using ParcelNET.Dhl.Shipping;
using ParcelNET.Dhl.Tracking;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-dhl-api-key";
    options.UseSandbox = true;  // Use sandbox for testing
})
.AddDhlShipping()
.AddDhlTracking();
```

### 2. Create a Shipment

```csharp
using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Shipping.Models;

public class ShippingService(IShipmentService shippingClient)
{
    public async Task<ShipmentResponse> CreateShipmentAsync()
    {
        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address
            {
                Name = "Sender GmbH",
                Street = "Senderstrasse",
                HouseNumber = "1",
                PostalCode = "10115",
                City = "Berlin",
                CountryCode = "DEU"
            },
            Consignee = new Address
            {
                Name = "Max Mustermann",
                Street = "Empfaengerstrasse",
                HouseNumber = "42",
                PostalCode = "80331",
                City = "Munich",
                CountryCode = "DEU"
            },
            Packages = [new Package { Weight = 2.5 }]
        };

        return await shippingClient.CreateShipmentAsync(request);
    }
}
```

### 3. Track a Shipment

```csharp
using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Models;

public class TrackingService(ITrackingService trackingClient)
{
    public async Task<TrackingResult> TrackAsync(string trackingNumber)
    {
        var result = await trackingClient.TrackAsync(trackingNumber);

        Console.WriteLine($"Status: {result.Status}");
        foreach (var evt in result.Events)
        {
            Console.WriteLine($"  {evt.Timestamp}: {evt.Description} ({evt.Location})");
        }

        return result;
    }
}
```

## Next Steps

- [Configuration](configuration) - Learn about DHL API configuration options
- [Error Handling](../guides/error-handling) - Handle API errors gracefully
- [Dependency Injection](../guides/dependency-injection) - Advanced DI patterns
