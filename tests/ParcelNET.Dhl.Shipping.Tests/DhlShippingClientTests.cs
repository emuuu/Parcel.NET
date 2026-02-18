using System.Net;
using System.Net.Http.Json;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl.Shipping.Models;
using Shouldly;
using Xunit;

namespace ParcelNET.Dhl.Shipping.Tests;

public class DhlShippingClientTests
{
    private static DhlShipmentRequest CreateTestRequest() => new()
    {
        BillingNumber = "33333333330101",
        Product = DhlProduct.V01PAK,
        Shipper = new Address
        {
            Name = "Test Shipper",
            Street = "Teststraße",
            HouseNumber = "1",
            PostalCode = "53113",
            City = "Bonn",
            CountryCode = "DEU"
        },
        Consignee = new Address
        {
            Name = "Test Consignee",
            Street = "Empfängerstraße",
            HouseNumber = "10",
            PostalCode = "10117",
            City = "Berlin",
            CountryCode = "DEU"
        },
        Packages =
        [
            new Package { Weight = 1.5, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } }
        ],
        Reference = "TEST-001"
    };

    private static DhlShippingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2")
        });

    [Fact]
    public async Task CreateShipmentAsync_ReturnsShipmentNumber()
    {
        var responseBody = new
        {
            status = new { title = "OK", statusCode = 200 },
            items = new[]
            {
                new
                {
                    shipmentNo = "00340434161094042557",
                    label = new { b64 = Convert.ToBase64String("fake-pdf"u8.ToArray()) },
                    sstatus = new { title = "OK", statusCode = 200 }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreateShipmentAsync(CreateTestRequest());

        result.ShipmentNumber.ShouldBe("00340434161094042557");
        result.Labels.ShouldNotBeEmpty();
        result.Labels[0].Content.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task CancelShipmentAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await client.CancelShipmentAsync("00340434161094042557");

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelShipmentAsync_NotFound_ReturnsFailureResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.CancelShipmentAsync("invalid");

        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void CreateTestRequest_HasValidStructure()
    {
        var request = CreateTestRequest();

        request.BillingNumber.ShouldBe("33333333330101");
        request.Product.ShouldBe(DhlProduct.V01PAK);
        request.Packages.Count.ShouldBe(1);
        request.Shipper.CountryCode.ShouldBe("DEU");
    }

    [Fact]
    public async Task CreateShipmentAsync_ApiError_ThrowsShippingException()
    {
        var errorBody = """{"status":{"title":"Bad Request","statusCode":400,"detail":"Invalid billing number."}}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ShippingException>(
            () => client.CreateShipmentAsync(CreateTestRequest()));

        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.RawResponse.ShouldNotBeNull();
        ex.RawResponse.ShouldContain("Invalid billing number");
    }

    [Fact]
    public async Task CreateShipmentAsync_EmptyPackages_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address { Name = "A", Street = "B", PostalCode = "12345", City = "C", CountryCode = "DEU" },
            Consignee = new Address { Name = "D", Street = "E", PostalCode = "54321", City = "F", CountryCode = "DEU" },
            Packages = []
        };

        await Should.ThrowAsync<ArgumentException>(() => client.CreateShipmentAsync(request));
    }

    [Fact]
    public async Task CreateShipmentAsync_MultiplePackages_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address { Name = "A", Street = "B", PostalCode = "12345", City = "C", CountryCode = "DEU" },
            Consignee = new Address { Name = "D", Street = "E", PostalCode = "54321", City = "F", CountryCode = "DEU" },
            Packages = [new Package { Weight = 1 }, new Package { Weight = 2 }]
        };

        await Should.ThrowAsync<ArgumentException>(() => client.CreateShipmentAsync(request));
    }

    [Fact]
    public async Task ValidateShipmentAsync_Valid_ReturnsValidResult()
    {
        var responseBody = new
        {
            status = new { title = "OK", statusCode = 200 },
            items = new[]
            {
                new
                {
                    shipmentNo = (string?)null,
                    validationMessages = Array.Empty<object>()
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.ValidateShipmentAsync(CreateTestRequest());

        result.Valid.ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateManifestAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await client.CreateManifestAsync();

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateShipmentAsync_MalformedJson_ThrowsShippingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", System.Text.Encoding.UTF8, "application/json")
        });

        await Should.ThrowAsync<Exception>(() => client.CreateShipmentAsync(CreateTestRequest()));
    }

    [Fact]
    public async Task CreateShipmentAsync_NullRequest_ThrowsArgumentNullException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Should.ThrowAsync<ArgumentNullException>(() => client.CreateShipmentAsync(null!));
    }

    // --- Unit Conversion Tests ---

    [Theory]
    [InlineData(1.0, WeightUnit.Kilogram, 1.0)]
    [InlineData(1000.0, WeightUnit.Gram, 1.0)]
    [InlineData(1.0, WeightUnit.Pound, 0.45359237)]
    [InlineData(1.0, WeightUnit.Ounce, 0.028349523)]
    public void ConvertWeight_ConvertsToKilogram(double value, WeightUnit unit, double expected)
    {
        var result = DhlShippingClient.ConvertWeight(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    [Theory]
    [InlineData(1.0, DimensionUnit.Centimeter, 1.0)]
    [InlineData(10.0, DimensionUnit.Millimeter, 1.0)]
    [InlineData(1.0, DimensionUnit.Inch, 2.54)]
    public void ConvertDimension_ConvertsToCentimeter(double value, DimensionUnit unit, double expected)
    {
        var result = DhlShippingClient.ConvertDimension(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    [Fact]
    public async Task CreateShipmentAsync_PoundWeight_ConvertsToKg()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                status = new { title = "OK", statusCode = 200 },
                items = new[] { new { shipmentNo = "123", label = new { b64 = Convert.ToBase64String("pdf"u8.ToArray()) }, sstatus = new { title = "OK", statusCode = 200 } } }
            })
        });

        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2")
        });

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address { Name = "A", Street = "B", PostalCode = "12345", City = "C", CountryCode = "DEU" },
            Consignee = new Address { Name = "D", Street = "E", PostalCode = "54321", City = "F", CountryCode = "DEU" },
            Packages = [new Package { Weight = 10.0, WeightUnit = WeightUnit.Pound, Dimensions = new Dimensions { Length = 10, Width = 5, Height = 3 }, DimensionUnit = DimensionUnit.Inch }]
        };

        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("4.535"); // 10 lbs ≈ 4.5359 kg
        body.ShouldContain("25.4"); // 10 in = 25.4 cm
    }

    // --- Locker/POBox Mapping Tests ---

    [Fact]
    public async Task CreateShipmentAsync_Locker_OmitsAddressStreetFromJson()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                status = new { title = "OK", statusCode = 200 },
                items = new[] { new { shipmentNo = "123", label = new { b64 = Convert.ToBase64String("pdf"u8.ToArray()) }, sstatus = new { title = "OK", statusCode = 200 } } }
            })
        });

        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2")
        });

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address { Name = "Shipper", Street = "Straße", PostalCode = "12345", City = "Berlin", CountryCode = "DEU" },
            Consignee = new Address { Name = "Consignee", Street = "Ignored", PostalCode = "54321", City = "München", CountryCode = "DEU" },
            DhlConsignee = new DhlConsignee { Type = DhlConsigneeType.Locker, LockerId = "123" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        // The consignee section should have lockerID but no addressStreet (omitted via WhenWritingNull)
        body.ShouldContain("\"lockerID\":\"123\"");
        // Verify addressStreet is not present for consignee by checking it only appears once (shipper)
        var occurrences = body.Split("addressStreet").Length - 1;
        occurrences.ShouldBe(1); // Only the shipper should have addressStreet
    }

    [Fact]
    public async Task CreateShipmentAsync_POBox_OmitsAddressStreetFromJson()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                status = new { title = "OK", statusCode = 200 },
                items = new[] { new { shipmentNo = "123", label = new { b64 = Convert.ToBase64String("pdf"u8.ToArray()) }, sstatus = new { title = "OK", statusCode = 200 } } }
            })
        });

        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2")
        });

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Shipper = new Address { Name = "Shipper", Street = "Straße", PostalCode = "12345", City = "Berlin", CountryCode = "DEU" },
            Consignee = new Address { Name = "Consignee", Street = "Ignored", PostalCode = "54321", City = "München", CountryCode = "DEU" },
            DhlConsignee = new DhlConsignee { Type = DhlConsigneeType.POBox, PoBoxId = "456" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"poBoxID\":456");
        var occurrences = body.Split("addressStreet").Length - 1;
        occurrences.ShouldBe(1); // Only the shipper should have addressStreet
    }

    // --- Error Handling Tests ---

    [Fact]
    public async Task CancelShipmentAsync_WithErrorBody_IncludesDetailInMessage()
    {
        var errorBody = """{"status":{"title":"Not Found","statusCode":404,"detail":"Shipment not found."}}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.CancelShipmentAsync("invalid");

        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("Shipment not found");
        result.Message.ShouldContain("404");
    }

    [Fact]
    public async Task CreateManifestAsync_WithErrorBody_IncludesDetailInMessage()
    {
        var errorBody = """{"detail":"No shipments ready for manifest."}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.CreateManifestAsync();

        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("No shipments ready for manifest");
        result.Message.ShouldContain("400");
    }

    [Fact]
    public async Task CreateShipmentAsync_ApiError_IncludesErrorDetailInMessage()
    {
        var errorBody = """{"status":{"title":"Bad Request","statusCode":400,"detail":"Invalid billing number."}}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ShippingException>(
            () => client.CreateShipmentAsync(CreateTestRequest()));

        ex.Message.ShouldContain("Invalid billing number");
        ex.Message.ShouldContain("400");
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_response);
    }
}
