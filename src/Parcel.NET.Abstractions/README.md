<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Abstractions

Shared interfaces and models for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — the carrier-agnostic foundation that all provider packages build on.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Abstractions.svg)](https://www.nuget.org/packages/Parcel.NET.Abstractions)

## Installation

```bash
dotnet add package Parcel.NET.Abstractions
```

## Key Types

| Type | Description |
|------|-------------|
| `IShipmentService` | Create and cancel shipments across carriers |
| `ITrackingService` | Track shipments across carriers |
| `ShipmentRequest` / `ShipmentResponse` | Carrier-agnostic shipment models |
| `TrackingResult` / `TrackingEvent` | Carrier-agnostic tracking models |
| `Address`, `Package`, `ContactInfo` | Shared value types |
| `ShippingException` / `TrackingException` | Typed exceptions with `StatusCode`, `ErrorCode`, `RawResponse` |

## Usage

Program against the abstractions for multi-carrier support:

```csharp
public class ShipmentController(IShipmentService shipmentService, ITrackingService trackingService)
{
    public async Task<string> Ship(ShipmentRequest request)
    {
        var response = await shipmentService.CreateShipmentAsync(request);
        return response.ShipmentNumber;
    }

    public async Task<TrackingResult> Track(string trackingNumber)
    {
        return await trackingService.TrackAsync(trackingNumber);
    }
}
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
