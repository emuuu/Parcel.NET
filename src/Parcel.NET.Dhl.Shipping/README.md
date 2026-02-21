![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.Dhl.Shipping

DHL Parcel DE Shipping API v2 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create, validate, and cancel shipments, and create daily closing manifests.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Shipping)

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
