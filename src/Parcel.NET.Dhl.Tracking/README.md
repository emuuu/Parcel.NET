![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg)

# Parcel.NET.Dhl.Tracking

DHL Parcel DE Tracking XML API v0 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track shipments and retrieve delivery signatures.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Tracking)

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Tracking
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
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

// Retrieve proof-of-delivery signature
byte[]? signature = await client.GetSignatureAsync("00340434161094042557");
```

`IDhlTrackingClient` extends `ITrackingService`. Sandbox mode is supported — set `UseSandbox = true` in `DhlOptions`.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
