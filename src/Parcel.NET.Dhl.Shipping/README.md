![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.Shipping

DHL Parcel DE Shipping API v2 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create, validate, and cancel shipments, and create daily closing manifests.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Shipping)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by Deutsche Post AG / DHL Group. See the trademark notice below.

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Shipping
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
.AddDhlShipping();

// Resolve the client
var client = serviceProvider.GetRequiredService<IDhlShippingClient>();

// Validate a shipment
var validation = await client.ValidateShipmentAsync(request);

// Create a shipment
var response = await client.CreateShipmentAsync(request);
Console.WriteLine($"Shipment: {response.ShipmentNumber}");

// Create daily closing manifest
var manifest = await client.CreateManifestAsync();

// Cancel a shipment
await client.CancelShipmentAsync(response.ShipmentNumber);
```

`IDhlShippingClient` extends `IShipmentService`, so you can also resolve it via the carrier-agnostic interface.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official DHL product and is not affiliated with, endorsed by, or sponsored by Deutsche Post AG / DHL Group. "DHL", "Deutsche Post", "Packstation", "Internetmarke", "Portokasse" and "E-POST" are trademarks or registered trademarks of Deutsche Post AG / DHL Group; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from DHL Group. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
