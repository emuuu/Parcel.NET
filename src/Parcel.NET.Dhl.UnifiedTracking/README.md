<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Dhl.UnifiedTracking

DHL Unified Tracking JSON API client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track DHL shipments across all services using the modern JSON API.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.UnifiedTracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.UnifiedTracking)

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
