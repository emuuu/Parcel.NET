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
        BillingNumber = "3333333333330102",
        Location = new PickupLocation
        {
            Address = new PickupAddress
            {
                Name1 = "Test GmbH",
                Street = "Teststra\u00dfe",
                HouseNumber = "1",
                PostalCode = "53113",
                City = "Bonn"
            }
        },
        PickupDate = new DateOnly(2026, 2, 20),
        TotalWeightInKg = 15.0,
        Shipments =
        [
            new PickupShipment { TransportationType = "PAKET", Size = "M" }
        ]
    };

    // ────────────────────────────────────────────────────────
    //  POST /orders
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePickupOrderAsync_ReturnsOrderId()
    {
        var responseBody = new
        {
            confirmation = new
            {
                type = "ORDERPICKUP",
                value = new
                {
                    orderID = "PO-12345",
                    pickupDate = "2026-02-20",
                    freeOfCharge = true,
                    pickupType = "BDA",
                    confirmedShipments = new[]
                    {
                        new { transportationType = "PAKET", shipmentNo = "00340434161094042557", orderDate = "2026-02-19T15:30:00" }
                    }
                }
            }
        };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CreatePickupOrderAsync(CreateTestRequest());

        result.OrderId.ShouldBe("PO-12345");
        result.PickupDate.ShouldBe(new DateOnly(2026, 2, 20));
        result.FreeOfCharge.ShouldBeTrue();
        result.PickupType.ShouldBe("BDA");
        result.ConfirmationType.ShouldBe("ORDERPICKUP");
        result.ConfirmedShipments.Count.ShouldBe(1);
        result.ConfirmedShipments[0].TransportationType.ShouldBe("PAKET");
        result.ConfirmedShipments[0].ShipmentNo.ShouldBe("00340434161094042557");
    }

    [Fact]
    public async Task CreatePickupOrderAsync_RequestUrl_IsOrders()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                confirmation = new
                {
                    type = "ORDERPICKUP",
                    value = new { orderID = "PO-001", pickupDate = "2026-02-20" }
                }
            })
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.CreatePickupOrderAsync(CreateTestRequest());

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/orders");
    }

    [Fact]
    public async Task CreatePickupOrderAsync_ValidateOnly_AppendsQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                confirmation = new
                {
                    type = "ORDERPICKUP",
                    value = new { orderID = "PO-002", pickupDate = "2026-02-20" }
                }
            })
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.CreatePickupOrderAsync(CreateTestRequest(), validateOnly: true);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.Query.ShouldContain("validate=true");
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

    [Fact]
    public async Task CreatePickupOrderAsync_LocationId_SetsTypeToId()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                confirmation = new
                {
                    type = "ORDERPICKUP",
                    value = new { orderID = "PO-003" }
                }
            })
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        var request = new PickupOrderRequest
        {
            BillingNumber = "3333333333330102",
            Location = new PickupLocation { LocationId = "AS3254120698" },
            PickupDate = new DateOnly(2026, 2, 20),
            Shipments = [new PickupShipment { TransportationType = "PAKET" }]
        };

        await client.CreatePickupOrderAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"type\":\"Id\"");
        body.ShouldContain("\"asId\":\"AS3254120698\"");
    }

    // ────────────────────────────────────────────────────────
    //  DELETE /orders?orderID=...
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelPickupOrdersAsync_ReturnsConfirmedCancellations()
    {
        var responseBody = new
        {
            confirmedCancellations = new[]
            {
                new { orderID = "PO-12345", orderState = "STORNIERT", message = "Cancelled." }
            },
            failedCancellations = Array.Empty<object>()
        };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CancelPickupOrdersAsync(["PO-12345"]);

        result.ConfirmedCancellations.Count.ShouldBe(1);
        result.ConfirmedCancellations[0].OrderId.ShouldBe("PO-12345");
        result.ConfirmedCancellations[0].OrderState.ShouldBe("STORNIERT");
        result.FailedCancellations.Count.ShouldBe(0);
    }

    [Fact]
    public async Task CancelPickupOrdersAsync_UsesQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                confirmedCancellations = Array.Empty<object>(),
                failedCancellations = Array.Empty<object>()
            })
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.CancelPickupOrdersAsync(["PO-001", "PO-002"]);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        var uri = handler.LastRequest.RequestUri!.ToString();
        uri.ShouldContain("orders?");
        uri.ShouldContain("orderID=PO-001");
        uri.ShouldContain("orderID=PO-002");
    }

    [Fact]
    public async Task CancelPickupOrdersAsync_NotFound_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"detail":"Order not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.CancelPickupOrdersAsync(["INVALID"]));
        ex.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ex.Message.ShouldContain("Order not found");
    }

    [Fact]
    public async Task CancelPickupOrdersAsync_EmptyList_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.CancelPickupOrdersAsync([]));
    }

    // ────────────────────────────────────────────────────────
    //  GET /orders?orderID=...
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPickupOrdersAsync_ReturnsOrderStatuses()
    {
        var responseBody = new[]
        {
            new { orderID = "PO-12345", orderState = "ANGENOMMEN", pickupDate = "2026-02-20" }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetPickupOrdersAsync(["PO-12345"]);

        result.Count.ShouldBe(1);
        result[0].OrderId.ShouldBe("PO-12345");
        result[0].OrderState.ShouldBe("ANGENOMMEN");
        result[0].PickupDate.ShouldBe(new DateOnly(2026, 2, 20));
    }

    [Fact]
    public async Task GetPickupOrdersAsync_UsesQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<object>())
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.GetPickupOrdersAsync(["PO-001"]);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        var uri = handler.LastRequest.RequestUri!.ToString();
        uri.ShouldContain("orders?");
        uri.ShouldContain("orderID=PO-001");
    }

    [Fact]
    public async Task GetPickupOrdersAsync_EmptyList_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.GetPickupOrdersAsync([]));
    }

    // ────────────────────────────────────────────────────────
    //  GET /locations
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPickupLocationsAsync_ReturnsLocations()
    {
        var responseBody = new[]
        {
            new
            {
                asId = "AS3254120698",
                pickupAddress = new
                {
                    name1 = "DHL Depot Bonn",
                    addressStreet = "Depotstr.",
                    addressHouse = "10",
                    postalCode = "53113",
                    city = "Bonn",
                    country = "DE"
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetPickupLocationsAsync("53113");

        result.Count.ShouldBe(1);
        result[0].LocationId.ShouldBe("AS3254120698");
        result[0].Address.ShouldNotBeNull();
        result[0].Address!.Name1.ShouldBe("DHL Depot Bonn");
        result[0].Address!.PostalCode.ShouldBe("53113");
    }

    [Fact]
    public async Task GetPickupLocationsAsync_WithPostalCode_AppendsQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<object>())
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.GetPickupLocationsAsync("53113");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("locations?postalCode=53113");
    }

    [Fact]
    public async Task GetPickupLocationsAsync_WithoutPostalCode_NoQueryParam()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<object>())
        });
        var client = new DhlPickupClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/transportation/pickup/v3/")
        });

        await client.GetPickupLocationsAsync();

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldEndWith("/locations");
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
