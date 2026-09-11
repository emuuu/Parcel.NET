![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Abstractions

Shared interfaces and models for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — the carrier-agnostic foundation that all provider packages build on.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Abstractions.svg)](https://www.nuget.org/packages/Parcel.NET.Abstractions)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by DHL Group, GO! Express & Logistics Deutschland GmbH, or A&O Fischer GmbH & Co. KG. See the trademark notice below.

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

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official product of any carrier and is not affiliated with, endorsed by, or sponsored by DHL Group, GO! Express & Logistics Deutschland GmbH, or A&O Fischer GmbH & Co. KG. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group. "GO!" and "GO! Express & Logistics" are trademarks or registered trademarks of GO! Express & Logistics Deutschland GmbH. "LetterXpress" and "SMART@MAIL" are trademarks or registered trademarks of A&O Fischer GmbH & Co. KG; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from the carriers. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
