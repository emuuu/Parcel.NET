using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using ParcelNET.GoExpress.Shipping.Models;
using Shouldly;
using Xunit;

namespace ParcelNET.GoExpress.Shipping.Tests;

public class GoExpressShippingClientTests
{
    private static readonly GoExpressOptions TestOptions = new()
    {
        Username = "testuser",
        Password = "testpass",
        CustomerId = "CUST01",
        ResponsibleStation = "FRA",
        UseSandbox = true
    };

    private static GoExpressShipmentRequest CreateTestRequest(
        GoExpressService service = GoExpressService.ON,
        GoExpressLabelFormat labelFormat = GoExpressLabelFormat.PdfA4,
        bool selfPickup = false,
        bool identCheck = false) => new()
    {
        Service = service,
        LabelFormat = labelFormat,
        SelfPickup = selfPickup,
        IdentCheck = identCheck,
        Pickup = new TimeWindow
        {
            Date = new DateOnly(2026, 3, 1),
            TimeFrom = new TimeOnly(8, 0),
            TimeTill = new TimeOnly(17, 0)
        },
        Shipper = new Address
        {
            Name = "Test Shipper",
            Street = "Teststraße",
            HouseNumber = "1",
            PostalCode = "60311",
            City = "Frankfurt",
            CountryCode = "DE"
        },
        Consignee = new Address
        {
            Name = "Test Consignee",
            Street = "Empfängerstraße",
            HouseNumber = "10",
            PostalCode = "10117",
            City = "Berlin",
            CountryCode = "DE"
        },
        Packages =
        [
            new Package { Weight = 1.5, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } }
        ],
        Reference = "TEST-001"
    };

    private static GoExpressShippingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci")
        }, Options.Create(TestOptions));

    [Fact]
    public async Task CreateShipmentAsync_ReturnsShipmentNumber()
    {
        var responseBody = new
        {
            hwbNumber = "GO1234567890",
            label = Convert.ToBase64String("fake-pdf"u8.ToArray())
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreateShipmentAsync(CreateTestRequest());

        result.ShipmentNumber.ShouldBe("GO1234567890");
        result.Labels.ShouldNotBeEmpty();
        result.Labels[0].Content.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task CreateShipmentAsync_ReturnsPdfLabel()
    {
        var pdfBytes = "fake-pdf-content"u8.ToArray();
        var responseBody = new
        {
            hwbNumber = "GO1234567890",
            label = Convert.ToBase64String(pdfBytes)
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = CreateTestRequest();
        var result = await client.CreateShipmentAsync(request);

        result.Labels[0].Format.ShouldBe(LabelFormat.Pdf);
        result.Labels[0].Content.ShouldBe(pdfBytes);
    }

    [Fact]
    public async Task CreateShipmentAsync_ReturnsZplLabel()
    {
        var zplContent = "^XA^FO50,50^ADN,36,20^FDHello^FS^XZ";
        var responseBody = new
        {
            hwbNumber = "GO1234567890",
            label = zplContent
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = CreateTestRequest(labelFormat: GoExpressLabelFormat.Zpl);
        var result = await client.CreateShipmentAsync(request);

        result.Labels[0].Format.ShouldBe(LabelFormat.Zpl);
        System.Text.Encoding.UTF8.GetString(result.Labels[0].Content).ShouldBe(zplContent);
    }

    [Fact]
    public async Task CreateShipmentAsync_ApiError_ThrowsShippingException()
    {
        var errorBody = """{"message":"Invalid customer ID."}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ShippingException>(
            () => client.CreateShipmentAsync(CreateTestRequest()));

        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.RawResponse.ShouldNotBeNull();
        ex.RawResponse.ShouldContain("Invalid customer ID");
    }

    [Fact]
    public async Task CreateShipmentAsync_NullRequest_ThrowsArgumentNullException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Should.ThrowAsync<ArgumentNullException>(() => client.CreateShipmentAsync(null!));
    }

    [Fact]
    public async Task CreateShipmentAsync_MapsServiceCorrectly()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "GO123",
                label = Convert.ToBase64String("pdf"u8.ToArray())
            })
        });

        var client = new GoExpressShippingClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci") },
            Options.Create(TestOptions));

        var request = CreateTestRequest(service: GoExpressService.INT);
        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"service\":\"INT\"");
    }

    [Fact]
    public async Task CreateShipmentAsync_MapsBooleanFieldsAsYesNo()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "GO123",
                label = Convert.ToBase64String("pdf"u8.ToArray())
            })
        });

        var client = new GoExpressShippingClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci") },
            Options.Create(TestOptions));

        var request = CreateTestRequest(selfPickup: true, identCheck: true);
        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"selfPickup\":\"Yes\"");
        body.ShouldContain("\"identCheck\":\"Yes\"");
        // FreightCollect = false should be omitted (null, WhenWritingNull)
        body.ShouldNotContain("\"freightCollect\"");
    }

    [Fact]
    public async Task CreateShipmentAsync_MapsWeightAsString()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "GO123",
                label = Convert.ToBase64String("pdf"u8.ToArray())
            })
        });

        var client = new GoExpressShippingClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci") },
            Options.Create(TestOptions));

        var request = CreateTestRequest();
        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"weight\":\"1.50\"");
    }

    [Fact]
    public async Task CancelShipmentAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await client.CancelShipmentAsync("GO1234567890");

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelShipmentAsync_NotFound_ReturnsFailureResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"HWB not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.CancelShipmentAsync("INVALID");

        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("HWB not found");
    }

    [Fact]
    public async Task GenerateLabelAsync_ReturnsLabel()
    {
        var pdfBytes = "label-content"u8.ToArray();
        var responseBody = new
        {
            label = Convert.ToBase64String(pdfBytes)
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GenerateLabelAsync("GO1234567890");

        result.Format.ShouldBe(LabelFormat.Pdf);
        result.Content.ShouldBe(pdfBytes);
    }

    // --- Unit Conversion Tests ---

    [Theory]
    [InlineData(1.0, WeightUnit.Kilogram, 1.0)]
    [InlineData(1000.0, WeightUnit.Gram, 1.0)]
    [InlineData(1.0, WeightUnit.Pound, 0.45359237)]
    [InlineData(1.0, WeightUnit.Ounce, 0.028349523)]
    public void ConvertWeight_ConvertsToKilogram(double value, WeightUnit unit, double expected)
    {
        var result = GoExpressShippingClient.ConvertWeight(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    [Theory]
    [InlineData(1.0, DimensionUnit.Centimeter, 1.0)]
    [InlineData(10.0, DimensionUnit.Millimeter, 1.0)]
    [InlineData(1.0, DimensionUnit.Inch, 2.54)]
    public void ConvertDimension_ConvertsToCentimeter(double value, DimensionUnit unit, double expected)
    {
        var result = GoExpressShippingClient.ConvertDimension(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    // --- Label Format Mapping Tests ---

    [Theory]
    [InlineData(GoExpressLabelFormat.Zpl, "1")]
    [InlineData(GoExpressLabelFormat.Pdf4x6, "2")]
    [InlineData(GoExpressLabelFormat.PdfA4, "4")]
    [InlineData(GoExpressLabelFormat.Tpcl, "5")]
    public void MapLabelFormat_ReturnsCorrectString(GoExpressLabelFormat format, string expected)
    {
        GoExpressShippingClient.MapLabelFormat(format).ShouldBe(expected);
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
