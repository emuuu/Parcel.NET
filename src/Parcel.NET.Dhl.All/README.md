<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Dhl.All

Meta-package that references all DHL packages in [Parcel.NET](https://github.com/emuuu/Parcel.NET). Install this single package to get the full DHL integration.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.All.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.All)

## Installation

```bash
dotnet add package Parcel.NET.Dhl.All
```

## Included Packages

| Package | Description |
|---------|-------------|
| `Parcel.NET.Dhl` | Core: configuration, auth, `DhlBuilder` |
| `Parcel.NET.Dhl.Shipping` | Parcel DE Shipping API v2 |
| `Parcel.NET.Dhl.Tracking` | Parcel DE Tracking XML API v0 |
| `Parcel.NET.Dhl.UnifiedTracking` | Unified Tracking JSON API |
| `Parcel.NET.Dhl.Pickup` | Parcel DE Pickup API v3 |
| `Parcel.NET.Dhl.Returns` | Parcel DE Returns API v1 |
| `Parcel.NET.Dhl.Internetmarke` | Post DE Internetmarke/Portokasse API v1 |
| `Parcel.NET.Dhl.LocationFinder` | Unified Location Finder API v1 |

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.Username = "your-username";
    options.Password = "your-password";
})
.AddDhlShipping()
.AddDhlTracking()
.AddDhlUnifiedTracking()
.AddDhlPickup()
.AddDhlReturns()
.AddDhlInternetmarke()
.AddDhlLocationFinder();
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
