using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Returns.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Returns.Tests;

public class DhlReturnsClientTests
{
    private static DhlReturnsClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/returns/v1/")
        });

    private static (DhlReturnsClient Client, MockHttpMessageHandler Handler) CreateClientWithHandler(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var client = new DhlReturnsClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/shipping/returns/v1/")
        });
        return (client, handler);
    }

    private static ReturnOrderRequest CreateValidRequest() => new()
    {
        ReceiverId = "deu",
        Shipper = new ReturnShipper
        {
            Name1 = "Max Mustermann",
            AddressStreet = "Teststraße",
            AddressHouse = "1",
            PostalCode = "53113",
            City = "Bonn",
            Country = "deu"
        }
    };

    [Fact]
    public async Task CreateReturnOrderAsync_ReturnsShipmentNumber()
    {
        var responseBody = new
        {
            shipmentNo = "RET-00340434161094042557",
            label = new { b64 = Convert.ToBase64String("fake-pdf"u8.ToArray()) },
            routingCode = "123456789",
            status = new { title = "OK", status = 201, detail = "Return label created." }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreateReturnOrderAsync(CreateValidRequest());

        result.ShipmentNo.ShouldBe("RET-00340434161094042557");
        result.LabelBase64.ShouldNotBeNull();
        result.RoutingCode.ShouldBe("123456789");
        result.StatusTitle.ShouldBe("OK");
        result.StatusDetail.ShouldBe("Return label created.");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_WithQrLabel_ReturnsBothLabels()
    {
        var responseBody = new
        {
            shipmentNo = "RET-001",
            label = new { b64 = "label-data" },
            qrLabel = new { b64 = "qr-data" },
            routingCode = "RC-001"
        };

        var request = CreateValidRequest();
        request.LabelType = ReturnLabelType.Both;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreateReturnOrderAsync(request);

        result.LabelBase64.ShouldBe("label-data");
        result.QrLabelBase64.ShouldBe("qr-data");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_LabelTypeIncludedAsQueryParam()
    {
        var responseBody = new
        {
            shipmentNo = "RET-001",
            label = new { b64 = "abc" }
        };

        var (client, handler) = CreateClientWithHandler(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = CreateValidRequest();
        request.LabelType = ReturnLabelType.QrLabel;

        await client.CreateReturnOrderAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("labelType=QR_LABEL");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_DefaultLabelType_IsBoth()
    {
        var responseBody = new
        {
            shipmentNo = "RET-001",
            label = new { b64 = "abc" }
        };

        var (client, handler) = CreateClientWithHandler(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseBody)
        });

        await client.CreateReturnOrderAsync(CreateValidRequest());

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("labelType=BOTH");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_RequestUrl_ContainsOrders()
    {
        var (client, handler) = CreateClientWithHandler(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { shipmentNo = "RET-001", label = new { b64 = "abc" } })
        });

        await client.CreateReturnOrderAsync(CreateValidRequest());

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("orders");
    }

    [Fact]
    public async Task GetReturnLocationsAsync_ReturnsList()
    {
        var responseBody = new[]
        {
            new
            {
                receiverId = "deu",
                billingNumber = "33333333330401",
                address = new
                {
                    name1 = "Postfiliale Bonn",
                    addressStreet = "Hauptstr.",
                    addressHouse = "1",
                    postalCode = "53113",
                    city = "Bonn",
                    country = "deu"
                }
            },
            new
            {
                receiverId = "deu",
                billingNumber = "33333333330402",
                address = new
                {
                    name1 = "Packstation 123",
                    addressStreet = "Bahnhofstr.",
                    addressHouse = "5",
                    postalCode = "53115",
                    city = "Bonn",
                    country = "deu"
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetReturnLocationsAsync("deu");

        result.Count.ShouldBe(2);
        result[0].ReceiverId.ShouldBe("deu");
        result[0].Name.ShouldBe("Postfiliale Bonn");
        result[0].Street.ShouldBe("Hauptstr.");
        result[0].PostalCode.ShouldBe("53113");
        result[1].Name.ShouldBe("Packstation 123");
    }

    [Fact]
    public async Task GetReturnLocationsAsync_WithOptionalParams_IncludesQueryParams()
    {
        var (client, handler) = CreateClientWithHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<object>())
        });

        await client.GetReturnLocationsAsync("deu", receiverId: "deu", billingNumber: "123", postalCode: "53113");

        handler.LastRequest.ShouldNotBeNull();
        var uri = handler.LastRequest!.RequestUri!.ToString();
        uri.ShouldContain("countryCode=deu");
        uri.ShouldContain("receiverId=deu");
        uri.ShouldContain("billingNumber=123");
        uri.ShouldContain("postalCode=53113");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"detail":"Invalid receiver ID."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.CreateReturnOrderAsync(CreateValidRequest()));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.Message.ShouldContain("Invalid receiver ID");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_NullRequest_ThrowsArgumentNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.CreateReturnOrderAsync(null!));
    }

    [Fact]
    public async Task GetReturnLocationsAsync_EmptyCountry_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.GetReturnLocationsAsync(""));
    }

    [Fact]
    public async Task GetReturnLocationsAsync_NullDeserialization_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        await Should.ThrowAsync<ParcelException>(() => client.GetReturnLocationsAsync("deu"));
    }

    [Fact]
    public async Task CreateReturnOrderAsync_MissingShipmentNo_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { shipmentNo = (string?)null, label = new { b64 = "abc" } })
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.CreateReturnOrderAsync(CreateValidRequest()));
        ex.Message.ShouldContain("shipment number");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_WithCustomsDetails_Succeeds()
    {
        var responseBody = new
        {
            shipmentNo = "RET-INTL-001",
            internationalShipmentNo = "INTL-001",
            label = new { b64 = "label-data" }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = CreateValidRequest();
        request.WeightInGrams = 1500;
        request.ItemValue = new ReturnItemValue { Currency = "EUR", Value = 99.99m };
        request.CustomsDetails = new ReturnCustomsDetails
        {
            Items =
            [
                new ReturnCustomsItem
                {
                    ItemDescription = "T-Shirt",
                    PackagedQuantity = 1,
                    WeightInGrams = 500,
                    ItemValue = new ReturnItemValue { Currency = "EUR", Value = 25.0m },
                    CountryOfOrigin = "deu",
                    HsCode = "610910"
                }
            ]
        };

        var result = await client.CreateReturnOrderAsync(request);

        result.ShipmentNo.ShouldBe("RET-INTL-001");
        result.InternationalShipmentNo.ShouldBe("INTL-001");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_WithShipmentLabelType_UsesCorrectQueryParam()
    {
        var (client, handler) = CreateClientWithHandler(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { shipmentNo = "RET-001", label = new { b64 = "abc" } })
        });

        var request = CreateValidRequest();
        request.LabelType = ReturnLabelType.ShipmentLabel;

        await client.CreateReturnOrderAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("labelType=SHIPMENT_LABEL");
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
