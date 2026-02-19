using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.UnifiedTracking.Tests;

public class DhlUnifiedTrackingClientTests
{
    private static DhlUnifiedTrackingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-eu.dhl.com/track/shipments")
        });

    [Fact]
    public async Task TrackAsync_ReturnsTrackingResult()
    {
        var responseBody = new
        {
            shipments = new[]
            {
                new
                {
                    id = "00340434161094042557",
                    status = new
                    {
                        statusCode = "transit",
                        status = "In transit",
                        description = "The shipment is in transit.",
                        timestamp = "2026-02-18T10:00:00+01:00"
                    },
                    estimatedTimeOfDelivery = "2026-02-20T12:00:00+01:00",
                    events = new[]
                    {
                        new
                        {
                            statusCode = "transit",
                            status = "In transit",
                            description = "The shipment has been processed.",
                            timestamp = "2026-02-18T10:00:00+01:00",
                            location = new
                            {
                                address = new { addressLocality = "Bonn", countryCode = "DE" }
                            }
                        }
                    }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.TrackAsync("00340434161094042557");

        result.ShipmentNumber.ShouldBe("00340434161094042557");
        result.Status.ShouldBe(TrackingStatus.InTransit);
        result.Events.Count.ShouldBe(1);
        result.Events[0].Location.ShouldBe("Bonn, DE");
        result.EstimatedDelivery.ShouldNotBeNull();
    }

    [Fact]
    public async Task TrackAsync_Delivered_MapsCorrectStatus()
    {
        var responseBody = new
        {
            shipments = new[]
            {
                new
                {
                    id = "00340434161094042557",
                    status = new { statusCode = "delivered" },
                    events = Array.Empty<object>()
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.TrackAsync("00340434161094042557");
        result.Status.ShouldBe(TrackingStatus.Delivered);
    }

    [Fact]
    public async Task TrackAsync_UnknownStatus_MapsToUnknown()
    {
        var responseBody = new
        {
            shipments = new[]
            {
                new
                {
                    id = "12345",
                    status = new { statusCode = "something-new" },
                    events = Array.Empty<object>()
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.TrackAsync("12345");
        result.Status.ShouldBe(TrackingStatus.Unknown);
    }

    [Fact]
    public async Task TrackAsync_HttpError_ThrowsTrackingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"title":"Not Found","status":404}""",
                System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("INVALID"));
        ex.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TrackAsync_EmptyShipmentsList_ThrowsTrackingException()
    {
        var responseBody = new { shipments = Array.Empty<object>() };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));
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

    [Fact]
    public async Task TrackAsync_WithOptions_IncludesQueryParams()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                shipments = new[]
                {
                    new
                    {
                        id = "12345",
                        status = new { statusCode = "transit" },
                        events = Array.Empty<object>()
                    }
                }
            })
        });

        var client = new DhlUnifiedTrackingClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-eu.dhl.com/track/shipments")
        });

        await client.TrackAsync("12345", new Models.DhlUnifiedTrackingOptions
        {
            Language = "de",
            RecipientPostalCode = "53113"
        });

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("language=de");
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("recipientPostalCode=53113");
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
