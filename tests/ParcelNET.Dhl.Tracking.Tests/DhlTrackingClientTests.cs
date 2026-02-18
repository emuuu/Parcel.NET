using System.Net;
using System.Net.Http.Json;
using ParcelNET.Abstractions.Exceptions;
using ParcelNET.Abstractions.Models;
using Shouldly;
using Xunit;

namespace ParcelNET.Dhl.Tracking.Tests;

public class DhlTrackingClientTests
{
    private static DhlTrackingClient CreateClient(HttpResponseMessage response) =>
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
                        },
                        new
                        {
                            statusCode = "pre-transit",
                            status = "Shipment picked up",
                            description = "The shipment has been picked up.",
                            timestamp = "2026-02-17T15:00:00+01:00",
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
        result.Events.Count.ShouldBe(2);
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
                    status = new
                    {
                        statusCode = "delivered",
                        status = "Delivered",
                        description = "The shipment has been delivered."
                    },
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
        ex.RawResponse.ShouldNotBeNull();
    }

    [Fact]
    public async Task TrackAsync_HttpError_IncludesErrorDetailInMessage()
    {
        var errorBody = """{"status":{"title":"Not Found","statusCode":404,"detail":"No shipment found for tracking number."},"title":"Not Found"}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("INVALID"));

        ex.Message.ShouldContain("No shipment found for tracking number");
        ex.Message.ShouldContain("404");
    }

    [Fact]
    public async Task TrackAsync_HttpErrorWithTopLevelDetail_IncludesDetailInMessage()
    {
        var errorBody = """{"detail":"Rate limit exceeded."}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("INVALID"));

        ex.Message.ShouldContain("Rate limit exceeded");
        ex.Message.ShouldContain("429");
    }

    [Fact]
    public async Task TrackAsync_HttpErrorWithNonJsonBody_IncludesRawBodyInMessage()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error", System.Text.Encoding.UTF8, "text/plain")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));

        ex.Message.ShouldContain("Internal Server Error");
        ex.Message.ShouldContain("500");
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
    public async Task TrackAsync_MissingTimestamp_UsesMinValue()
    {
        var responseBody = new
        {
            shipments = new[]
            {
                new
                {
                    id = "12345",
                    status = new { statusCode = "transit" },
                    events = new[]
                    {
                        new
                        {
                            statusCode = "transit",
                            description = "In transit",
                            timestamp = (string?)null,
                            location = (object?)null
                        }
                    }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.TrackAsync("12345");

        result.Events.Count.ShouldBe(1);
        result.Events[0].Timestamp.ShouldBe(DateTimeOffset.MinValue);
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
