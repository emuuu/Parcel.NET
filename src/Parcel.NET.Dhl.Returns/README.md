<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Dhl.Returns

DHL Parcel DE Returns API v1 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — create return orders and look up return locations.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Returns.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Returns)

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
