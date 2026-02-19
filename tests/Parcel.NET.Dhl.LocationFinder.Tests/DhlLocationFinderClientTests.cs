using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.LocationFinder.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.LocationFinder.Tests;

public class DhlLocationFinderClientTests
{
    private static DhlLocationFinderClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api.dhl.com/location-finder/v1/")
        });

    [Fact]
    public async Task FindByAddressAsync_ReturnsLocations()
    {
        var responseBody = new
        {
            locations = new[]
            {
                new
                {
                    name = "Packstation 123",
                    location = new
                    {
                        ids = new[] { new { locationId = "LOC-001", provider = "parcel" } },
                        type = "packstation"
                    },
                    place = new
                    {
                        address = new { streetAddress = "Hauptstr. 1", postalCode = "53113", addressLocality = "Bonn", countryCode = "DE" },
                        geo = new { latitude = 50.7374, longitude = 7.0982 }
                    },
                    serviceTypes = new[] { "parcel:pick-up", "parcel:drop-off" }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.FindByAddressAsync("DE", "Bonn", "53113");

        result.Locations.Count.ShouldBe(1);
        result.Locations[0].Name.ShouldBe("Packstation 123");
        result.Locations[0].Type.ShouldBe("packstation");
        result.Locations[0].PostalCode.ShouldBe("53113");
        result.Locations[0].Latitude.ShouldBe(50.7374);
    }

    [Fact]
    public async Task FindByGeoAsync_ReturnsLocations()
    {
        var responseBody = new
        {
            locations = new[]
            {
                new
                {
                    name = "Postfiliale",
                    distance = 250.0,
                    location = new
                    {
                        ids = new[] { new { locationId = "LOC-002", provider = "parcel" } },
                        type = "postoffice"
                    },
                    place = new
                    {
                        address = new { streetAddress = "Bahnhofstr. 5", postalCode = "53115", addressLocality = "Bonn", countryCode = "DE" },
                        geo = new { latitude = 50.73, longitude = 7.10 }
                    },
                    serviceTypes = new[] { "parcel:pick-up" }
                }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.FindByGeoAsync(50.7374, 7.0982, 500);

        result.Locations.Count.ShouldBe(1);
        result.Locations[0].DistanceInMeters.ShouldBe(250.0);
        result.Locations[0].Type.ShouldBe("postoffice");
    }

    [Fact]
    public async Task GetLocationByIdAsync_ReturnsLocation()
    {
        var responseBody = new
        {
            name = "Packstation 456",
            location = new
            {
                ids = new[] { new { locationId = "LOC-456", provider = "parcel" } },
                type = "packstation"
            },
            place = new
            {
                address = new { streetAddress = "Am Markt 3", postalCode = "10117", addressLocality = "Berlin", countryCode = "DE" },
                geo = new { latitude = 52.52, longitude = 13.405 }
            },
            serviceTypes = new[] { "parcel:pick-up", "parcel:drop-off" }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetLocationByIdAsync("LOC-456");

        result.Id.ShouldBe("LOC-456");
        result.Name.ShouldBe("Packstation 456");
        result.City.ShouldBe("Berlin");
    }

    [Fact]
    public async Task FindByAddressAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"detail":"Invalid country code."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.FindByAddressAsync("XX", "Nowhere"));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FindByAddressAsync_EmptyCountry_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.FindByAddressAsync("", "Bonn"));
    }

    [Fact]
    public async Task FindByAddressAsync_EmptyCity_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.FindByAddressAsync("DE", ""));
    }

    [Fact]
    public async Task FindByGeoAsync_GermanLocale_UsesInvariantCultureInUrl()
    {
        var responseBody = new { locations = Array.Empty<object>() };
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });
        var client = new DhlLocationFinderClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.dhl.com/location-finder/v1/")
        });

        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            await client.FindByGeoAsync(52.5, 13.4);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }

        var url = handler.LastRequest!.RequestUri!.ToString();
        url.ShouldContain("latitude=52.5");
        url.ShouldContain("longitude=13.4");
        url.ShouldNotContain("52,5");
        url.ShouldNotContain("13,4");
    }

    [Fact]
    public async Task FindByAddressAsync_RequestUrl_ContainsCorrectPath()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { locations = Array.Empty<object>() })
        });
        var client = new DhlLocationFinderClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.dhl.com/location-finder/v1/")
        });

        await client.FindByAddressAsync("DE", "Bonn", "53113");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        var url = handler.LastRequest.RequestUri!.ToString();
        url.ShouldContain("find-by-address");
        url.ShouldContain("countryCode=DE");
        url.ShouldContain("addressLocality=Bonn");
        url.ShouldContain("postalCode=53113");
    }

    [Fact]
    public async Task FindByGeoAsync_NullDeserialization_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        await Should.ThrowAsync<ParcelException>(() => client.FindByGeoAsync(50.0, 7.0));
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
