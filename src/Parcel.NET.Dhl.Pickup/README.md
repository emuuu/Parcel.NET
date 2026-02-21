<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Dhl.Pickup

DHL Parcel DE Pickup API v3 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create, cancel, and query pickup orders.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Pickup.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Pickup)

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
