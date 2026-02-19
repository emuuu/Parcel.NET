using Microsoft.Extensions.DependencyInjection;
using ParcelNET.Abstractions;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl;
using ParcelNET.Dhl.Shipping;
using ParcelNET.Dhl.Shipping.Models;
using ParcelNET.Dhl.Tracking;
using ParcelNET.GoExpress;
using ParcelNET.GoExpress.Shipping;
using ParcelNET.GoExpress.Shipping.Models;
using ParcelNET.GoExpress.Tracking;

// Setup DI
var services = new ServiceCollection();

services.AddDhl(options =>
{
    options.ApiKey = "YOUR_DHL_API_KEY";
    options.ApiSecret = "YOUR_DHL_API_SECRET";
    options.Username = "YOUR_DHL_USERNAME";
    options.Password = "YOUR_DHL_PASSWORD";
    options.UseSandbox = true;
})
.AddDhlShipping()
.AddDhlTracking();

services.AddGoExpress(options =>
{
    options.Username = "YOUR_GO_USERNAME";
    options.Password = "YOUR_GO_PASSWORD";
    options.CustomerId = "CUST01";
    options.ResponsibleStation = "FRA";
    options.UseSandbox = true;
})
.AddGoExpressShipping()
.AddGoExpressTracking();

await using var provider = services.BuildServiceProvider();

// -- DHL Shipping Example --
Console.WriteLine("=== DHL Shipping Example ===");

var dhlShipping = provider.GetRequiredService<IDhlShippingClient>();

var dhlRequest = new DhlShipmentRequest
{
    BillingNumber = "33333333330101",
    Product = DhlProduct.V01PAK,
    Shipper = new Address
    {
        Name = "Max Mustermann",
        Street = "Musterstraße",
        HouseNumber = "1",
        PostalCode = "53113",
        City = "Bonn",
        CountryCode = "DEU"
    },
    Consignee = new Address
    {
        Name = "Erika Musterfrau",
        Street = "Berliner Straße",
        HouseNumber = "42",
        PostalCode = "10117",
        City = "Berlin",
        CountryCode = "DEU"
    },
    Packages =
    [
        new Package
        {
            Weight = 2.5,
            Dimensions = new Dimensions { Length = 35, Width = 25, Height = 10 }
        }
    ],
    Reference = "ORDER-12345",
    ShipDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
};

Console.WriteLine($"Creating shipment for {dhlRequest.Consignee.Name}...");
Console.WriteLine("(This would call the DHL API in a real scenario)");

try
{
    // Uncomment to call the DHL API:
    // var shipmentResponse = await dhlShipping.CreateShipmentAsync(dhlRequest);
    // Console.WriteLine($"Shipment created: {shipmentResponse.ShipmentNumber}");
}
catch (ShippingException ex)
{
    Console.WriteLine($"Shipping error: {ex.Message}");
}

// -- DHL Tracking Example --
Console.WriteLine();
Console.WriteLine("=== DHL Tracking Example ===");

var trackingService = provider.GetRequiredService<ITrackingService>();

Console.WriteLine("Tracking shipment 00340434161094042557...");
Console.WriteLine("(This would call the DHL API in a real scenario)");

try
{
    // Uncomment to call the DHL API:
    // var trackingResult = await trackingService.TrackAsync("00340434161094042557");
    // Console.WriteLine($"Status: {trackingResult.Status}");
}
catch (TrackingException ex)
{
    Console.WriteLine($"Tracking error: {ex.Message}");
}

// -- GO! Express Shipping Example --
Console.WriteLine();
Console.WriteLine("=== GO! Express Shipping Example ===");

var goShipping = provider.GetRequiredService<IGoExpressShippingClient>();

var goRequest = new GoExpressShipmentRequest
{
    Service = GoExpressService.ON,
    Pickup = new TimeWindow
    {
        Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        TimeFrom = new TimeOnly(8, 0),
        TimeTill = new TimeOnly(17, 0)
    },
    Shipper = new Address
    {
        Name = "Max Mustermann",
        Street = "Musterstraße",
        HouseNumber = "1",
        PostalCode = "60311",
        City = "Frankfurt",
        CountryCode = "DE"
    },
    Consignee = new Address
    {
        Name = "Erika Musterfrau",
        Street = "Berliner Straße",
        HouseNumber = "42",
        PostalCode = "10117",
        City = "Berlin",
        CountryCode = "DE"
    },
    Packages =
    [
        new Package
        {
            Weight = 2.5,
            Dimensions = new Dimensions { Length = 35, Width = 25, Height = 10 }
        }
    ],
    Reference = "ORDER-67890"
};

Console.WriteLine($"Creating GO! Express shipment for {goRequest.Consignee.Name}...");
Console.WriteLine("(This would call the GO! Express API in a real scenario)");

try
{
    // Uncomment to call the GO! Express API:
    // var goResponse = await goShipping.CreateShipmentAsync(goRequest);
    // Console.WriteLine($"Shipment created: {goResponse.ShipmentNumber}");
}
catch (ShippingException ex)
{
    Console.WriteLine($"Shipping error: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("Setup complete. Replace credentials and uncomment API calls to use.");
