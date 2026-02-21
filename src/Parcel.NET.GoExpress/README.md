<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.GoExpress

Core GO! Express package for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — provides `GoExpressOptions` configuration, Basic Auth handling, and the fluent `GoExpressBuilder` used to register individual GO! Express service clients.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress)

## Installation

```bash
dotnet add package Parcel.NET.GoExpress
```

## Usage

```csharp
builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
    options.UseSandbox = true;
})
.AddGoExpressShipping()  // requires Parcel.NET.GoExpress.Shipping
.AddGoExpressTracking(); // requires Parcel.NET.GoExpress.Tracking
```

`AddGoExpress()` returns a `GoExpressBuilder` that the individual service packages chain onto. Authentication (HTTP Basic Auth) is handled automatically.

## Configuration

| Property | Required | Description |
|----------|----------|-------------|
| `Username` | Yes | Basic Auth username |
| `Password` | Yes | Basic Auth password |
| `CustomerId` | Yes | Customer ID (max 7 characters) |
| `ResponsibleStation` | For Shipping | Station code (3 characters, e.g. `"FRA"`) |
| `UseSandbox` | No | Use sandbox endpoints |

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
