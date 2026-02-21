<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.GoExpress.Shipping

GO! Express Shipping client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create and cancel shipments and generate labels.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Shipping)

## Installation

```bash
dotnet add package Parcel.NET.GoExpress.Shipping
```

## Usage

```csharp
builder.Services.AddGoExpress(options =>
{
    options.Username = "your-username";
    options.Password = "your-password";
    options.CustomerId = "1234567";
    options.ResponsibleStation = "FRA";
})
.AddGoExpressShipping();

var client = serviceProvider.GetRequiredService<IGoExpressShippingClient>();

// Create a shipment
var response = await client.CreateShipmentAsync(request);
Console.WriteLine($"HWB: {response.ShipmentNumber}");

// Generate a label
var label = await client.GenerateLabelAsync(response.ShipmentNumber, GoExpressLabelFormat.PdfA4);

// Cancel a shipment
await client.CancelShipmentAsync(response.ShipmentNumber);
```

`IGoExpressShippingClient` extends `IShipmentService`, so you can also resolve it via the carrier-agnostic interface.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
