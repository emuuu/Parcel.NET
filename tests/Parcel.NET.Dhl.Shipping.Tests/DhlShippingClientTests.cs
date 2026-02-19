using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Shipping.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Shipping.Tests;

public class DhlShippingClientTests
{
    private static DhlShipmentRequest CreateTestRequest() => new()
    {
        BillingNumber = "33333333330101",
        Profile = "STANDARD_GRUPPENPROFIL",
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
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
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
    public async Task CreateShipmentAsync_SendsProfileInBody()
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
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CreateShipmentAsync(CreateTestRequest());

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"profile\":\"STANDARD_GRUPPENPROFIL\"");
    }

    [Fact]
    public async Task CreateShipmentAsync_DimensionsAreIntegers()
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
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CreateShipmentAsync(CreateTestRequest());

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        // Dimensions should be integers: 30, 20, 15
        body.ShouldContain("\"length\":30");
        body.ShouldContain("\"width\":20");
        body.ShouldContain("\"height\":15");
    }

    [Fact]
    public async Task CancelShipmentAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var result = await client.CancelShipmentAsync("00340434161094042557");
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelShipmentAsync_IncludesProfileQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CancelShipmentAsync("12345");

        handler.LastRequest!.RequestUri!.ToString().ShouldContain("profile=");
        handler.LastRequest.RequestUri.ToString().ShouldContain("shipment=12345");
    }

    [Fact]
    public async Task CancelShipmentAsync_WithProfile_UsesSpecifiedProfile()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CancelShipmentAsync("12345", "MY_PROFILE");

        handler.LastRequest!.RequestUri!.ToString().ShouldContain("profile=MY_PROFILE");
    }

    [Fact]
    public async Task CancelShipmentAsync_NotFound_ThrowsShippingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"detail":"Shipment not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ShippingException>(() => client.CancelShipmentAsync("invalid"));
        ex.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public void CreateTestRequest_HasValidStructure()
    {
        var request = CreateTestRequest();

        request.BillingNumber.ShouldBe("33333333330101");
        request.Profile.ShouldBe("STANDARD_GRUPPENPROFIL");
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
            Profile = "STANDARD_GRUPPENPROFIL",
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
            Profile = "STANDARD_GRUPPENPROFIL",
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
    public async Task CreateManifestAsync_SendsProfileInBody()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CreateManifestAsync("MY_PROFILE");

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"profile\":\"MY_PROFILE\"");
    }

    [Fact]
    public async Task CreateManifestAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var result = await client.CreateManifestAsync();
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateShipmentAsync_NullRequest_ThrowsArgumentNullException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.CreateShipmentAsync(null!));
    }

    // --- Consignee Type Tests ---

    [Fact]
    public async Task CreateShipmentAsync_Locker_UsesCorrectApiFormat()
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
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Profile = "STANDARD_GRUPPENPROFIL",
            Shipper = new Address { Name = "Shipper", Street = "Straße", PostalCode = "12345", City = "Berlin", CountryCode = "DEU" },
            Consignee = new Address { Name = "Consignee", PostalCode = "54321", City = "München", CountryCode = "DEU" },
            DhlConsignee = new DhlConsignee { Type = DhlConsigneeType.Locker, LockerId = 123, PostNumber = "1234567890" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        // Locker uses "name" (not "name1"), integer lockerID, and postNumber
        body.ShouldContain("\"lockerID\":123");
        body.ShouldContain("\"postNumber\":\"1234567890\"");
    }

    [Fact]
    public async Task CreateShipmentAsync_POBox_UsesCorrectApiFormat()
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
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        var request = new DhlShipmentRequest
        {
            BillingNumber = "33333333330101",
            Profile = "STANDARD_GRUPPENPROFIL",
            Shipper = new Address { Name = "Shipper", Street = "Straße", PostalCode = "12345", City = "Berlin", CountryCode = "DEU" },
            Consignee = new Address { Name = "Consignee", PostalCode = "54321", City = "München", CountryCode = "DEU" },
            DhlConsignee = new DhlConsignee { Type = DhlConsigneeType.POBox, PoBoxId = 456 },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"poBoxID\":456");
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

    // --- Error Handling Tests ---

    [Fact]
    public async Task CancelShipmentAsync_EmptyShipmentNumber_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.CancelShipmentAsync(""));
    }

    [Fact]
    public async Task CancelShipmentAsync_RequestUrl_ContainsShipmentNumber()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CancelShipmentAsync("00340434161094042557");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("shipment=00340434161094042557");
    }

    [Fact]
    public async Task CreateManifestAsync_RequestUrl_IsManifests()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new DhlShippingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/v2/")
        });

        await client.CreateManifestAsync();

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("manifests");
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
