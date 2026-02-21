![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.GoExpress.All

Meta-package that references all GO! Express packages in [Parcel.NET](https://github.com/emuuu/Parcel.NET). Install this single package to get the full GO! Express integration.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.All.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.All)

## Installation

```bash
dotnet add package Parcel.NET.GoExpress.All
```

## Included Packages

| Package | Description |
|---------|-------------|
| `Parcel.NET.GoExpress` | Core: configuration, Basic Auth, `GoExpressBuilder` |
| `Parcel.NET.GoExpress.Shipping` | Shipping client — create/cancel shipments, labels |
| `Parcel.NET.GoExpress.Tracking` | Tracking client — track by HWB number |

## Usage

```csharp
builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
})
.AddGoExpressShipping()
.AddGoExpressTracking();
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
