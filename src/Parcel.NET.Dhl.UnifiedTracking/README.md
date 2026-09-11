![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.UnifiedTracking

DHL Unified Tracking JSON API client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track DHL shipments across all services using the modern JSON API.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.UnifiedTracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.UnifiedTracking)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl.UnifiedTracking
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
})
.AddDhlUnifiedTracking();

var client = serviceProvider.GetRequiredService<IDhlUnifiedTrackingClient>();

// Track a shipment
var result = await client.TrackAsync("00340434161094042557");
Console.WriteLine($"Status: {result.Status}");

// Track with filtering options
var result = await client.TrackAsync("00340434161094042557",
    new DhlUnifiedTrackingOptions
    {
        Language = "en",
        Service = "parcel-de",
        RecipientPostalCode = "10117"
    });
```

`IDhlUnifiedTrackingClient` extends `ITrackingService`. Only requires an API key — no additional credentials needed.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
