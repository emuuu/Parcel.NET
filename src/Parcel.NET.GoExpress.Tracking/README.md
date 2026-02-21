<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.GoExpress.Tracking

GO! Express Tracking client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — track shipments by HWB number.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Tracking.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Tracking)

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
