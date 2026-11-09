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

For DHL integration, install the base package and the specific service packages you need:

```bash
dotnet add package Parcel.NET.Dhl
dotnet add package Parcel.NET.Dhl.Shipping
dotnet add package Parcel.NET.Dhl.Tracking
```

### GO! Express Packages

For GO! Express integration:

```bash
dotnet add package Parcel.NET.GoExpress
dotnet add package Parcel.NET.GoExpress.Shipping
dotnet add package Parcel.NET.GoExpress.Tracking
```

### Package Overview

| Package | Description |
|---------|-------------|
| `Parcel.NET.Abstractions` | Core interfaces (`IShipmentService`, `ITrackingService`) and shared models |
| **DHL** | |
| `Parcel.NET.Dhl` | DHL authentication, configuration, and DI extensions |
| `Parcel.NET.Dhl.Shipping` | DHL Parcel DE Shipping API v2 client |
| `Parcel.NET.Dhl.Tracking` | DHL Parcel DE Tracking API v0 (XML) client |
| **GO! Express** | |
| `Parcel.NET.GoExpress` | GO! Express authentication, configuration, and DI extensions |
| `Parcel.NET.GoExpress.Shipping` | GO! Express Shipping client |
| `Parcel.NET.GoExpress.Tracking` | GO! Express Tracking client |

## Supported Frameworks

| Framework | Status |
|-----------|--------|
| .NET 10.0 | Supported (LTS) |

## Package References

If you prefer editing your `.csproj` directly, replace `X.Y.Z` with the current version from [nuget.org](https://www.nuget.org/packages/Parcel.NET.Dhl.Tracking) (each package has its own page). `dotnet add package` selects the latest version automatically.

**DHL:**

```xml
<ItemGroup>
  <PackageReference Include="Parcel.NET.Dhl.Shipping" Version="X.Y.Z" />
  <PackageReference Include="Parcel.NET.Dhl.Tracking" Version="X.Y.Z" />
</ItemGroup>
```

**GO! Express:**

```xml
<ItemGroup>
  <PackageReference Include="Parcel.NET.GoExpress.Shipping" Version="X.Y.Z" />
  <PackageReference Include="Parcel.NET.GoExpress.Tracking" Version="X.Y.Z" />
</ItemGroup>
```
