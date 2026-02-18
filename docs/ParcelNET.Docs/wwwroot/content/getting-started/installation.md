---
title: Installation
category: Getting Started
order: 1
description: How to install ParcelNET packages via NuGet.
---

## NuGet Packages

ParcelNET is distributed as a set of NuGet packages. Install only the carrier packages you need.

### Core Package

The abstractions package contains carrier-agnostic interfaces and models:

```bash
dotnet add package ParcelNET.Abstractions
```

### DHL Packages

For DHL integration, install the DHL meta-package (includes both shipping and tracking):

```bash
dotnet add package ParcelNET.Dhl
dotnet add package ParcelNET.Dhl.Shipping
dotnet add package ParcelNET.Dhl.Tracking
```

### Package Overview

| Package | Description |
|---------|-------------|
| `ParcelNET.Abstractions` | Core interfaces (`IShipmentService`, `ITrackingService`) and shared models |
| `ParcelNET.Dhl` | DHL authentication, configuration, and DI extensions |
| `ParcelNET.Dhl.Shipping` | DHL Parcel DE Shipping API v2 client |
| `ParcelNET.Dhl.Tracking` | DHL Shipment Tracking API v1 client |

## Supported Frameworks

| Framework | Status |
|-----------|--------|
| .NET 10.0 | Supported (LTS) |

## Package References

If you prefer editing your `.csproj` directly:

```xml
<ItemGroup>
  <PackageReference Include="ParcelNET.Dhl.Shipping" Version="0.1.0-alpha" />
  <PackageReference Include="ParcelNET.Dhl.Tracking" Version="0.1.0-alpha" />
</ItemGroup>
```
