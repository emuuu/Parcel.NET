---
title: Quick Start
category: Getting Started
order: 2
description: Get up and running with Parcel.NET in minutes.
---

## DHL Quick Start

This example demonstrates registering DHL services, creating a shipment, and tracking it.

### 1. Register Services

```csharp
using Parcel.NET.Dhl;
using Parcel.NET.Dhl.Shipping;
using Parcel.NET.Dhl.Tracking;

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
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Shipping.Models;

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
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Models;

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

## GO! Express Quick Start

This example demonstrates registering GO! Express services, creating a shipment, and tracking it.

### 1. Register Services

```csharp
using Parcel.NET.GoExpress;
using Parcel.NET.GoExpress.Shipping;
using Parcel.NET.GoExpress.Tracking;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
    options.UseSandbox = true;
})
.AddGoExpressShipping()
.AddGoExpressTracking();
```

### 2. Create a Shipment

```csharp
using Parcel.NET.Abstractions.Models;
using Parcel.NET.GoExpress.Shipping;
using Parcel.NET.GoExpress.Shipping.Models;

public class GoShippingService(IGoExpressShippingClient shippingClient)
{
    public async Task<ShipmentResponse> CreateShipmentAsync()
    {
        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            Pickup = new TimeWindow
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                TimeFrom = new TimeOnly(8, 0),
                TimeTill = new TimeOnly(17, 0)
            },
            Shipper = new Address
            {
                Name = "Sender GmbH",
                Street = "Senderstrasse",
                HouseNumber = "1",
                PostalCode = "60311",
                City = "Frankfurt",
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
            Packages = [new Package { Weight = 5.0 }]
        };

        return await shippingClient.CreateShipmentAsync(request);
    }
}
```

### 3. Track a Shipment

```csharp
using Parcel.NET.Abstractions;
using Parcel.NET.Abstractions.Models;

public class GoTrackingService(ITrackingService trackingClient)
{
    public async Task<TrackingResult> TrackAsync(string hwbNumber)
    {
        var result = await trackingClient.TrackAsync(hwbNumber);

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

- [Configuration](configuration) — Configure carrier API credentials and environments
- [Error Handling](../guides/error-handling) — Handle API errors gracefully
- [Dependency Injection](../guides/dependency-injection) — Advanced DI patterns
