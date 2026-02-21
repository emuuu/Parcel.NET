<p align="center">
  <img src="https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg" alt="Parcel.NET" width="128" />
</p>

# Parcel.NET.Dhl.LocationFinder

DHL Unified Location Finder API v1 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — search for DHL service points, parcel lockers, and post offices by address, geo-coordinates, or keyword.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.LocationFinder.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.LocationFinder)

## Installation

```bash
dotnet add package Parcel.NET.Dhl.LocationFinder
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
})
.AddDhlLocationFinder();

var client = serviceProvider.GetRequiredService<IDhlLocationFinderClient>();

// Find by address
var results = await client.FindByAddressAsync("DEU", "Bonn", "53113");

// Find by geo-coordinates (radius in meters)
var results = await client.FindByGeoAsync(50.7374, 7.0982, 5000);

// Find by keyword ID
var results = await client.FindByKeywordAsync("packstation");

// Get a specific location
var location = await client.GetLocationByIdAsync("8003-4012221");
```

Only requires a DHL API key — no additional credentials needed.

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
