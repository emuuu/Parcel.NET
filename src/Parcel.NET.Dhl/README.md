![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl

Core DHL package for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — provides `DhlOptions` configuration, OAuth token management, and the fluent `DhlBuilder` used to register individual DHL service clients.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.Username = "your-username";
    options.Password = "your-password";
    options.UseSandbox = true;
})
.AddDhlShipping()      // requires Parcel.NET.Dhl.Shipping
.AddDhlTracking()      // requires Parcel.NET.Dhl.Tracking
.AddDhlUnifiedTracking()
.AddDhlPickup()
.AddDhlReturns()
.AddDhlInternetmarke()
.AddDhlLocationFinder();
```

`AddDhl()` returns a `DhlBuilder` that the individual service packages chain onto. Authentication (OAuth ROPC or API-key header) is handled automatically per service.

## Configuration

| Property | Required | Description |
|----------|----------|-------------|
| `ApiKey` | Yes | DHL API key (also used as OAuth `client_id`) |
| `ApiSecret` | For Shipping/Pickup/Returns/Internetmarke | OAuth `client_secret` |
| `Username` / `Password` | For Shipping/Pickup/Returns | ROPC credentials |
| `TrackingUsername` / `TrackingPassword` | For Tracking (XML) | Parcel DE Tracking credentials |
| `InternetmarkeUsername` / `InternetmarkePassword` | For Internetmarke | Falls back to `Username`/`Password` |
| `UseSandbox` | No | Use sandbox endpoints |

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
