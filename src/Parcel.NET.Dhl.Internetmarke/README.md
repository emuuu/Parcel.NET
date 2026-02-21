![Parcel.NET](https://raw.githubusercontent.com/emuuu/Parcel.NET/main/parcelNET-logo.svg)

# Parcel.NET.Dhl.Internetmarke

DHL Post DE Internetmarke/Portokasse API v1 client for [Parcel.NET](https://github.com/emuuu/Parcel.NET) — purchase digital stamps, browse the product catalog, manage your wallet, and process retoures.

[![NuGet](https://img.shields.io/nuget/v/Parcel.NET.Dhl.Internetmarke.svg)](https://www.nuget.org/packages/Parcel.NET.Dhl.Internetmarke)

## Installation

```bash
dotnet add package Parcel.NET.Dhl.Internetmarke
```

## Usage

```csharp
builder.Services.AddDhl(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-api-secret";
    options.InternetmarkeUsername = "your-username";
    options.InternetmarkePassword = "your-password";
})
.AddDhlInternetmarke();

var client = serviceProvider.GetRequiredService<IDhlInternetmarkeClient>();

// Get user profile
var profile = await client.GetUserProfileAsync();

// Browse product catalog
var catalog = await client.GetCatalogAsync();

// Cart-based stamp purchase
var cart = await client.InitializeCartAsync();
var checkout = await client.CheckoutCartPdfAsync(new CheckoutRequest { /* ... */ });

// Charge wallet (Portokasse)
await client.ChargeWalletAsync(new WalletChargeRequest { /* ... */ });
```

## Links

- [Full documentation](https://emuuu.github.io/Parcel.NET/)
- [GitHub repository](https://github.com/emuuu/Parcel.NET)
