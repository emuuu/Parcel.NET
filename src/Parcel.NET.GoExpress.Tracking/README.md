![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.GoExpress.Tracking

GO! Express Tracking client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track shipments by HWB number.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Tracking)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by GO! Express & Logistics Deutschland GmbH. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.GoExpress.Tracking
```

## Usage

```csharp
builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
})
.AddGoExpressTracking();

var trackingService = serviceProvider.GetRequiredService<ITrackingService>();

var result = await trackingService.TrackAsync("GO123456789");
Console.WriteLine($"Status: {result.Status}");
foreach (var evt in result.Events)
{
    Console.WriteLine($"  {evt.Timestamp}: {evt.Description}");
}
```

Registers directly as `ITrackingService` — use the carrier-agnostic interface to resolve the client.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official GO! Express & Logistics product and is not affiliated with, endorsed by, or sponsored by GO! Express & Logistics Deutschland GmbH. "GO!" and "GO! Express & Logistics" are trademarks or registered trademarks of GO! Express & Logistics Deutschland GmbH; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from GO! Express & Logistics. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
