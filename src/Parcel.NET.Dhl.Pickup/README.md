![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.Pickup

DHL Parcel DE Pickup API v3 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create, cancel, and query pickup orders.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Pickup.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Pickup)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Pickup
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
.AddDhlPickup();

var client = serviceProvider.GetRequiredService<IDhlPickupClient>();

// Create a pickup order
var order = await client.CreatePickupOrderAsync(new PickupOrderRequest { /* ... */ });

// Query pickup orders
var orders = await client.GetPickupOrdersAsync(["ORDER-123"]);

// Get pickup locations
var locations = await client.GetPickupLocationsAsync("53113");

// Cancel pickup orders
await client.CancelPickupOrdersAsync(["ORDER-123"]);
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
