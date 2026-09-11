![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.png)

# Parcel.NET.GoExpress.Shipping

GO! Express Shipping client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create and cancel shipments and generate labels.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.GoExpress.Shipping.svg)](https://www.nuget.org/packages/Parcel.NET.GoExpress.Shipping)

> **Unofficial project.** Not affiliated with, endorsed by, or supported by GO! Express & Logistics Deutschland GmbH. See the trademark notice below.

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

## Trademarks and disclaimer

This is an independent, community-maintained project. It is not an official GO! Express & Logistics product and is not affiliated with, endorsed by, or sponsored by GO! Express & Logistics Deutschland GmbH. "GO!" and "GO! Express & Logistics" are trademarks or registered trademarks of GO! Express & Logistics Deutschland GmbH; all other names and brands are the property of their respective owners and are used solely to identify the third-party APIs this package communicates with. Provided under the MIT license, as-is, with no warranty and no support agreement from GO! Express & Logistics. Details: [Parcel.NET](https://github.com/emuuu/Parcel.NET).
