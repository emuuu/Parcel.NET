---
title: Installation
category: Getting Started
order: 1
description: How to install Parcel.NET packages via NuGet.
---

## NuGet Packages

Parcel.NET is distributed as a set of NuGet packages. Install only the carrier packages you need.

### Core Package

The abstractions package contains carrier-agnostic interfaces and models:

```bash
dotnet add package Parcel.NET.Abstractions
```

### DHL Packages

For DHL integration, install the DHL meta-package (includes both shipping and tracking):

```bash
dotnet add package Parcel.NET.Dhl
dotnet add package Parcel.NET.Dhl.Shipping
dotnet add package Parcel.NET.Dhl.Tracking
```

### Package Overview

| Package | Description |
|---------|-------------|
| `Parcel.NET.Abstractions` | Core interfaces (`IShipmentService`, `ITrackingService`) and shared models |
| `Parcel.NET.Dhl` | DHL authentication, configuration, and DI extensions |
| `Parcel.NET.Dhl.Shipping` | DHL Parcel DE Shipping API v2 client |
| `Parcel.NET.Dhl.Tracking` | DHL Shipment Tracking API v1 client |

## Supported Frameworks

| Framework | Status |
|-----------|--------|
| .NET 10.0 | Supported (LTS) |

## Package References

If you prefer editing your `.csproj` directly:

```xml
<ItemGroup>
  <PackageReference Include="Parcel.NET.Dhl.Shipping" Version="0.1.0-alpha" />
  <PackageReference Include="Parcel.NET.Dhl.Tracking" Version="0.1.0-alpha" />
</ItemGroup>
```
