![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.Returns

DHL Parcel DE Returns API v1 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create return orders and look up return locations.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Returns.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Returns)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Returns
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.Username = "your-username";
    options.Password = "your-password";
})
.AddDhlReturns();

var client = serviceProvider.GetRequiredService<IDhlReturnsClient>();

// Create a return order
var returnOrder = await client.CreateReturnOrderAsync(new ReturnOrderRequest { /* ... */ });

// List return locations for a country
var locations = await client.GetReturnLocationsAsync("DEU");
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
