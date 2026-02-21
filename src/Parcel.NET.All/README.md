![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg)

# Parcel.NET.All

Meta-package that references every package in [Parcel.NET](https://github.com/emuuu/Parcel.NET). Install this single package to get full DHL and GO! Express support.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.All.svg)](https://www.nuget.org/packages/Parcel.NET.All)

## Installation

```bash
dotnet add package Parcel.NET.All
```

## Included Packages

**DHL** — via `Parcel.NET.Dhl.All`:

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

**GO! Express** — via `Parcel.NET.GoExpress.All`:

| Package | Description |
|---------|-------------|
| `Parcel.NET.GoExpress` | Core: configuration, Basic Auth, `GoExpressBuilder` |
| `Parcel.NET.GoExpress.Shipping` | Shipping client — create/cancel shipments, labels |
| `Parcel.NET.GoExpress.Tracking` | Tracking client — track by HWB number |

**Shared:**

| Package | Description |
|---------|-------------|
| `Parcel.NET.Abstractions` | Carrier-agnostic interfaces and models |

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
