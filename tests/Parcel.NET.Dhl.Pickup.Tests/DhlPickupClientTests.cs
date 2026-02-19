using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Pickup.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Pickup.Tests;

public class DhlPickupClientTests
{
    private static DhlPickupClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

    private static PickupOrderRequest CreateTestRequest() => new()
    {
        Address = new PickupAddress
        {
            Name = "Test GmbH",
            Street = "Teststraße",
            HouseNumber = "1",
            PostalCode = "53113",
            City = "Bonn"
        },
        Contact = new PickupContact { Name = "Max Mustermann", Phone = "+49123456789" },
        PickupFrom = new DateTimeOffset(2026, 2, 20, 8, 0, 0, TimeSpan.FromHours(1)),
        PickupUntil = new DateTimeOffset(2026, 2, 20, 17, 0, 0, TimeSpan.FromHours(1)),
        PackageCount = 3,
        TotalWeightInKg = 15.0
    };

    [Fact]
    public async Task CreatePickupOrderAsync_ReturnsOrderNumber()
    {
        var responseBody = new { orderNumber = "PO-12345", status = "CONFIRMED" };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreatePickupOrderAsync(CreateTestRequest());

        result.OrderNumber.ShouldBe("PO-12345");
        result.Status.ShouldBe("CONFIRMED");
    }

    [Fact]
    public async Task CancelPickupOrderAsync_Success()
    {
        var responseBody = new { message = "Order cancelled." };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CancelPickupOrderAsync("PO-12345");

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelPickupOrderAsync_NotFound_ReturnsFailure()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"detail":"Order not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.CancelPickupOrderAsync("INVALID");

        result.Success.ShouldBeFalse();
        result.Message!.ShouldContain("Order not found");
    }

    [Fact]
    public async Task GetPickupOrderAsync_ReturnsDetails()
    {
        var responseBody = new
        {
            orderNumber = "PO-12345",
            status = "CONFIRMED",
            pickupAddress = new { name = "Test GmbH", street = "Teststr.", houseNumber = "1", postalCode = "53113", city = "Bonn", country = "DE" },
            contactPerson = new { name = "Max", phone = "+49123" },
            pickupDetails = new { pickupDate = "2026-02-20", readyByTime = "08:00", closingTime = "17:00", totalPackages = 3, totalWeight = 15.0 }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetPickupOrderAsync("PO-12345");

        result.OrderNumber.ShouldBe("PO-12345");
        result.Status.ShouldBe("CONFIRMED");
        result.PackageCount.ShouldBe(3);
    }

    [Fact]
    public async Task CreatePickupOrderAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"detail":"Invalid pickup date."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.CreatePickupOrderAsync(CreateTestRequest()));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.Message.ShouldContain("Invalid pickup date");
    }

    [Fact]
    public async Task CreatePickupOrderAsync_NullRequest_ThrowsArgumentNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.CreatePickupOrderAsync(null!));
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
