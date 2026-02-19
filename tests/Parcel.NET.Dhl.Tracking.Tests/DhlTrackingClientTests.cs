using System.Net;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Tracking.Tests;

public class DhlTrackingClientTests
{
    private static readonly DhlOptions TestOptions = new()
    {
        ApiKey = "test-api-key",
        TrackingUsername = "zt12345",
        TrackingPassword = "geheim",
        UseSandbox = true
    };

    private static IOptions<DhlOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(TestOptions);

    private static DhlTrackingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments")
        }, CreateOptions());

    [Fact]
    public async Task TrackAsync_ReturnsTrackingResult_WithDataNameFormat()
    {
        // Real DHL API format: all elements are <data> tags with name attributes
        var xml = """
            <data code="0" name="piece-shipment-list">
                <data name="piece-shipment" piece-code="00340434161094042557" piece-status="5" delivery-event-flag="1" delivery-date="2026-02-18">
                    <data name="piece-event-list" piece-identifier="00340434161094042557">
                        <data name="piece-event" event-timestamp="2026-02-18T10:00:00" event-location="Bonn" event-country="DE" event-status="Delivered" event-text="Die Sendung wurde zugestellt." standard-event-code="DLVRD"/>
                    </data>
                </data>
            </data>
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var result = await client.TrackAsync("00340434161094042557");

        result.ShipmentNumber.ShouldBe("00340434161094042557");
        result.Status.ShouldBe(TrackingStatus.Delivered);
        result.Events.Count.ShouldBe(1);
        result.Events[0].Location.ShouldBe("Bonn, DE");
        result.Events[0].StatusCode.ShouldBe("DLVRD");
    }

    [Fact]
    public async Task TrackAsync_EstimatedDelivery_IsParsed()
    {
        var xml = """
            <data code="0" name="piece-shipment-list">
                <data name="piece-shipment" piece-code="12345" piece-status="2" delivery-event-flag="0" delivery-date="2026-02-20">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="2026-02-18T08:00:00" event-status="In transit" event-text="Transit" standard-event-code="TRNS"/>
                    </data>
                </data>
            </data>
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var result = await client.TrackAsync("12345");
        result.EstimatedDelivery.ShouldNotBeNull();
        result.EstimatedDelivery!.Value.Date.ShouldBe(new DateTime(2026, 2, 20));
    }

    [Fact]
    public async Task TrackAsync_InTransit_MapsCorrectStatus()
    {
        var xml = """
            <data code="0" name="piece-shipment-list">
                <data name="piece-shipment" piece-code="12345" piece-status="2" delivery-event-flag="0">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="2026-02-18T08:00:00" event-status="In transit" event-text="Sendung in Transit." standard-event-code="TRNS"/>
                    </data>
                </data>
            </data>
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var result = await client.TrackAsync("12345");
        result.Status.ShouldBe(TrackingStatus.InTransit);
    }

    [Fact]
    public async Task TrackAsync_OutForDelivery_MapsCorrectStatus()
    {
        var xml = """
            <data code="0" name="piece-shipment-list">
                <data name="piece-shipment" piece-code="12345" piece-status="4" delivery-event-flag="0">
                    <data name="piece-event-list"/>
                </data>
            </data>
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var result = await client.TrackAsync("12345");
        result.Status.ShouldBe(TrackingStatus.OutForDelivery);
    }

    [Fact]
    public async Task TrackAsync_ErrorResponse_ThrowsTrackingException()
    {
        var xml = """<data code="1" error="No data found for this piece code."/>""";

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("INVALID"));
        ex.Message.ShouldContain("No data found");
    }

    [Fact]
    public async Task TrackAsync_HttpError_ThrowsTrackingException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", System.Text.Encoding.UTF8, "text/plain")
        });

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrackAsync_NullTrackingNumber_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.TrackAsync(null!));
    }

    [Fact]
    public void BuildXmlQuery_IncludesCredentialsAndPieceCode()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var xml = client.BuildXmlQuery("d-get-piece-detail", "00340434161094042557", null);

        xml.ShouldContain("appname=\"zt12345\"");
        xml.ShouldContain("password=\"geheim\"");
        xml.ShouldContain("piece-code=\"00340434161094042557\"");
        xml.ShouldContain("request=\"d-get-piece-detail\"");
        xml.ShouldContain("language-code=\"de\"");
    }

    [Fact]
    public void BuildXmlQuery_WithOptions_IncludesZipCode()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var xml = client.BuildXmlQuery("d-get-piece-detail", "12345", new Models.DhlTrackingOptions { ZipCode = "53113" });

        xml.ShouldContain("zip-code=\"53113\"");
    }

    [Fact]
    public void BuildXmlQuery_WithLanguage_OverridesDefault()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var xml = client.BuildXmlQuery("d-get-piece-detail", "12345", new Models.DhlTrackingOptions { Language = "en" });

        xml.ShouldContain("language-code=\"en\"");
    }

    [Fact]
    public async Task TrackPublicAsync_UsesPublicRequestType()
    {
        // Real DHL format uses <data name="..."> wrappers
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                <data code="0">
                    <data piece-code="12345" piece-status="2" delivery-event-flag="0">
                        <data name="piece-status-public-list">
                            <data name="piece-status-public" event-timestamp="2026-02-18T08:00:00" event-status="In transit" event-text="In transit"/>
                        </data>
                    </data>
                </data>
                """, System.Text.Encoding.UTF8, "application/xml")
        });

        var client = new DhlTrackingClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments") },
            CreateOptions());

        var result = await client.TrackPublicAsync("12345");

        result.Events.Count.ShouldBe(1);
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("get-status-for-public-user");
    }

    [Fact]
    public async Task GetSignatureAsync_ReturnsBytes_FromHexEncoding()
    {
        // DHL returns signature images as hex-encoded byte data, not Base64
        var imageBytes = "fake-gif-data"u8.ToArray();
        var imageHex = Convert.ToHexString(imageBytes);
        var xml = $"""<data code="0"><data image="{imageHex}"/></data>""";

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var bytes = await client.GetSignatureAsync("12345");
        bytes.ShouldNotBeNull();
        bytes!.Length.ShouldBeGreaterThan(0);
        System.Text.Encoding.UTF8.GetString(bytes).ShouldBe("fake-gif-data");
    }

    [Fact]
    public async Task GetSignatureAsync_ErrorCode_ReturnsNull()
    {
        var xml = """<data code="1" error="No signature available."/>""";

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var bytes = await client.GetSignatureAsync("12345");
        bytes.ShouldBeNull();
    }

    [Fact]
    public void BuildXmlQuery_XmlInjectionAttempt_IsEscaped()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var xml = client.BuildXmlQuery("d-get-piece-detail", """12345" malicious="true""", null);

        xml.ShouldNotContain("malicious=\"true\"");
        xml.ShouldContain("piece-code=");
        System.Xml.Linq.XElement.Parse(xml);
    }

    [Fact]
    public async Task TrackAsync_EmptyEventList_ReturnsEmptyEvents()
    {
        var xml = """
            <data code="0" name="piece-shipment-list">
                <data name="piece-shipment" piece-code="12345" piece-status="0" delivery-event-flag="0">
                    <data name="piece-event-list"/>
                </data>
            </data>
            """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        });

        var result = await client.TrackAsync("12345");
        result.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task TrackAsync_RequestUrl_ContainsXmlQueryParameter()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                <data code="0" name="piece-shipment-list">
                    <data name="piece-shipment" piece-code="12345" piece-status="2" delivery-event-flag="0">
                        <data name="piece-event-list">
                            <data name="piece-event" event-timestamp="2026-02-18T08:00:00" event-status="In transit" event-text="Transit" standard-event-code="TRNS"/>
                        </data>
                    </data>
                </data>
                """, System.Text.Encoding.UTF8, "application/xml")
        });

        var client = new DhlTrackingClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments") },
            CreateOptions());

        await client.TrackAsync("12345");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.Query.ShouldContain("xml=");
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
