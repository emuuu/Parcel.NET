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

    [Fact]
    public async Task CreateReturnOrderAsync_ReturnsShipmentNumber()
    {
        var responseBody = new
        {
            shipmentNumber = "RET-00340434161094042557",
            labelData = Convert.ToBase64String("fake-pdf"u8.ToArray()),
            routingCode = "123456789"
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = new ReturnOrderRequest
        {
            ReceiverId = "RET-12345",
            SenderAddress = new ReturnAddress
            {
                Name = "Max Mustermann",
                Street = "Teststraße",
                HouseNumber = "1",
                PostalCode = "53113",
                City = "Bonn",
                CountryCode = "DEU"
            }
        };

        var result = await client.CreateReturnOrderAsync(request);

        result.ShipmentNumber.ShouldBe("RET-00340434161094042557");
        result.LabelPdf.ShouldNotBeNull();
        result.RoutingCode.ShouldBe("123456789");
    }

    [Fact]
    public async Task GetReturnLocationsAsync_ReturnsList()
    {
        var responseBody = new
        {
            locations = new[]
            {
                new { id = "LOC-1", name = "Postfiliale Bonn", street = "Hauptstr. 1", postalCode = "53113", city = "Bonn", countryCode = "DE" },
                new { id = "LOC-2", name = "Packstation 123", street = "Bahnhofstr. 5", postalCode = "53115", city = "Bonn", countryCode = "DE" }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetReturnLocationsAsync("DEU");

        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Postfiliale Bonn");
    }

    [Fact]
    public async Task CreateReturnOrderAsync_ApiError_ThrowsShippingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"detail":"Invalid receiver ID."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var request = new ReturnOrderRequest
        {
            ReceiverId = "INVALID",
            SenderAddress = new ReturnAddress { Name = "A", Street = "B", PostalCode = "12345", City = "C", CountryCode = "DEU" }
        };

        var ex = await Should.ThrowAsync<ShippingException>(() => client.CreateReturnOrderAsync(request));
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
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
