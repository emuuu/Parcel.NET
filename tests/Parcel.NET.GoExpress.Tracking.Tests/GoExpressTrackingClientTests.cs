using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.GoExpress.Tracking.Tests;

public class GoExpressTrackingClientTests
{
    private static GoExpressTrackingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/api/v1/")
        });

    private static GoExpressTrackingClient CreateClientWithHandler(MockHttpMessageHandler handler) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/api/v1/")
        });

    // --- Basic Tests ---

    [Fact]
    public async Task TrackAsync_ReturnsTrackingResult()
    {
        var response = new
        {
            trackingItems = new
            {
                reference = "999999000001",
                recipientZipCode = "36272",
                transportStation = new { name = "HOQ", city = "Hof" },
                transportStatus = "GO50",
                locationInfo = "Delivered",
                locationOverview = new[]
                {
                    new
                    {
                        name = "Hof",
                        trackingStatuses = new[]
                        {
                            new { status = "Sendung zugestellt", statusCode = "GO50", statusDate = "2025-08-02T10:30:00" }
                        }
                    }
                },
                trackingTable = new[]
                {
                    new { status = "Auftrag erfasst", statusCode = "GO10", statusDate = "2025-08-01T15:00:00", station = "Bonn", remarks = "" },
                    new { status = "In Zustellung", statusCode = "GO42", statusDate = "2025-08-02T08:00:00", station = "Hof", remarks = "" },
                    new { status = "Sendung zugestellt", statusCode = "GO50", statusDate = "2025-08-02T10:30:00", station = "Hof", remarks = "Empfänger" }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(response)
        });

        var result = await client.TrackAsync("999999000001");

        result.ShipmentNumber.ShouldBe("999999000001");
        result.Status.ShouldBe(TrackingStatus.Delivered);
        result.Events.Count.ShouldBe(3);
    }

    [Fact]
    public async Task TrackAsync_MapsEventsCorrectly()
    {
        var response = new
        {
            trackingItems = new
            {
                reference = "123",
                transportStatus = "GO42",
                trackingTable = new[]
                {
                    new { status = "Auftrag erfasst", statusCode = "GO10", statusDate = "2025-08-01T15:00:00", station = "Bonn", remarks = "" },
                    new { status = "In Zustellung", statusCode = "GO42", statusDate = "2025-08-02T08:00:00", station = "Hof", remarks = "Fahrer unterwegs" }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(response)
        });

        var result = await client.TrackAsync("123");

        result.Events[0].Description.ShouldBe("Auftrag erfasst");
        result.Events[0].StatusCode.ShouldBe("GO10");
        result.Events[0].Location.ShouldBe("Bonn");
        result.Events[0].Timestamp.ShouldNotBeNull();

        result.Events[1].Description.ShouldBe("In Zustellung");
        result.Events[1].StatusCode.ShouldBe("GO42");
        result.Events[1].Location.ShouldBe("Hof");
    }

    // --- Status Mapping Tests ---

    [Theory]
    [InlineData("GO10", TrackingStatus.PreTransit)]
    [InlineData("GO20", TrackingStatus.InTransit)]
    [InlineData("GO40", TrackingStatus.InTransit)]
    [InlineData("GO42", TrackingStatus.OutForDelivery)]
    [InlineData("GO50", TrackingStatus.Delivered)]
    [InlineData("GO90", TrackingStatus.Returned)]
    [InlineData("UNKNOWN", TrackingStatus.Unknown)]
    [InlineData(null, TrackingStatus.Unknown)]
    public async Task TrackAsync_MapsTransportStatus(string? statusCode, TrackingStatus expected)
    {
        var response = new
        {
            trackingItems = new
            {
                reference = "123",
                transportStatus = statusCode,
                trackingTable = Array.Empty<object>()
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(response)
        });

        var result = await client.TrackAsync("123");
        result.Status.ShouldBe(expected);
    }

    // --- Request URL Tests ---

    [Fact]
    public async Task TrackAsync_UsesCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                trackingItems = new
                {
                    reference = "999999000001",
                    transportStatus = "GO10",
                    trackingTable = Array.Empty<object>()
                }
            })
        });

        var client = CreateClientWithHandler(handler);
        await client.TrackAsync("999999000001");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        var url = handler.LastRequest.RequestUri!.ToString();
        url.ShouldContain("status?language=de&hwbNumber=999999000001");
    }

    [Fact]
    public async Task TrackAsync_EscapesAmpersandInHwbNumber()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                trackingItems = new
                {
                    reference = "test",
                    transportStatus = "GO10",
                    trackingTable = Array.Empty<object>()
                }
            })
        });

        var client = CreateClientWithHandler(handler);
        await client.TrackAsync("12345&test");

        var url = handler.LastRequest!.RequestUri!.ToString();
        // & must be escaped so it doesn't start a new query parameter
        url.ShouldContain("hwbNumber=12345%26test");
        url.ShouldNotContain("hwbNumber=12345&test");
    }

    // --- Error Handling Tests ---

    [Fact]
    public async Task TrackAsync_HttpError_ThrowsTrackingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", System.Text.Encoding.UTF8, "text/plain")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("123"));
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrackAsync_NotFound_ThrowsTrackingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"HWB not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("INVALID"));
        ex.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ex.RawResponse.ShouldNotBeNull();
        ex.RawResponse!.ShouldContain("HWB not found");
    }

    [Fact]
    public async Task TrackAsync_NullTrackingNumber_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.TrackAsync(null!));
    }

    [Fact]
    public async Task TrackAsync_EmptyTrackingNumber_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.TrackAsync(""));
    }

    // --- Edge Cases ---

    [Fact]
    public async Task TrackAsync_EmptyTrackingTable_ReturnsEmptyEvents()
    {
        var response = new
        {
            trackingItems = new
            {
                reference = "123",
                transportStatus = "GO10",
                trackingTable = Array.Empty<object>()
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(response)
        });

        var result = await client.TrackAsync("123");
        result.Events.ShouldBeEmpty();
        result.Status.ShouldBe(TrackingStatus.PreTransit);
    }

    [Fact]
    public async Task TrackAsync_NullTrackingItems_ThrowsTrackingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });

        // Should not throw - just return unknown status with empty events
        var result = await client.TrackAsync("123");
        result.Status.ShouldBe(TrackingStatus.Unknown);
        result.Events.ShouldBeEmpty();
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
