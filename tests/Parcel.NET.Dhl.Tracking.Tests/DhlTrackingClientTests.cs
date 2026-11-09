using System.Globalization;
using System.Net;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Shouldly;
using Xunit;
using static Parcel.NET.Dhl.Tracking.Tests.TestSupport;

namespace Parcel.NET.Dhl.Tracking.Tests;

public class DhlTrackingClientTests
{
    // --- d-get-piece-detail: real DHL sandbox response and derived states ---

    [Fact]
    public async Task TrackAsync_DeliveredSandboxResponse_ParsesShipmentAndEvents()
    {
        var client = CreateClientWithFixture("piece-detail-delivered");

        var result = await client.TrackAsync("00340434161094042557");

        result.ShipmentNumber.ShouldBe("00340434161094042557");
        result.Status.ShouldBe(TrackingStatus.Delivered);
        result.Events.Count.ShouldBe(7);
        result.Events[0].StatusCode.ShouldBe("ES");
        result.Events[0].Location.ShouldBe("Bremen, Germany");
        result.Events[0].Timestamp.ShouldBe(new DateTimeOffset(2016, 3, 17, 11, 21, 0, TimeSpan.FromHours(1)));
        result.Events[5].StatusCode.ShouldBe("PO");
        result.Events[6].StatusCode.ShouldBe("ZU");
        result.Events[6].Description.ShouldBe("The shipment has been successfully delivered");
        result.Events[6].Location.ShouldBe("Bonn, Germany");
    }

    [Theory]
    [InlineData("piece-detail-announced", TrackingStatus.PreTransit)]       // VA / PARCV
    [InlineData("piece-detail-in-transit", TrackingStatus.InTransit)]       // AA / LDTMV
    [InlineData("piece-detail-at-branch", TrackingStatus.InTransit)]        // ZF / HLDCC (waiting for pickup)
    [InlineData("piece-detail-out-for-delivery", TrackingStatus.OutForDelivery)] // PO / SRTED
    [InlineData("piece-detail-delivery-failed", TrackingStatus.Exception)]  // ZN / NTDEL
    [InlineData("piece-detail-returned", TrackingStatus.Returned)]          // ruecksendung="true"
    [InlineData("piece-detail-delivered", TrackingStatus.Delivered)]        // ZU / DLVRD
    public async Task TrackAsync_Fixture_MapsStatus(string fixture, TrackingStatus expected)
    {
        var client = CreateClientWithFixture(fixture);

        var result = await client.TrackAsync("00340434161094042557");

        result.Status.ShouldBe(expected);
    }

    [Fact]
    public async Task TrackAsync_EventListAsSiblingOfShipment_ParsesEventsAndStatus()
    {
        var client = CreateClientWithFixture("piece-detail-sibling-event-list");

        var result = await client.TrackAsync("266334453");

        result.ShipmentNumber.ShouldBe("266334453");
        result.Status.ShouldBe(TrackingStatus.Delivered);
        result.Events.Count.ShouldBe(2);
        result.Events[0].Location.ShouldBe("Saulheim, Deutschland");
        result.Events[1].StatusCode.ShouldBe("ZU");
    }

    [Fact]
    public async Task TrackAsync_ShipmentWithoutIceAndEventCode_UsesLastEvent()
    {
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0" ice="" standard-event-code="" ruecksendung="false">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="17.03.2016 11:21" event-text="Posted" ice="SHRCU" ric="PCKST" standard-event-code="ES"/>
                        <data name="piece-event" event-timestamp="18.03.2016 09:02" event-text="In delivery" ice="SRTED" ric="NRQRD" standard-event-code="PO"/>
                    </data>
                </data>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Status.ShouldBe(TrackingStatus.OutForDelivery);
    }

    [Fact]
    public async Task TrackAsync_ShipmentWithoutIceAndEventCode_UsesReturnFlagOfLastEvent()
    {
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="18.03.2016 09:02" event-text="Return" ice="LDTMV" ric="MVMTV" standard-event-code="AA" ruecksendung="true"/>
                    </data>
                </data>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Status.ShouldBe(TrackingStatus.Returned);
    }

    [Fact]
    public async Task TrackAsync_ShipmentAttributesWinOverLastEvent()
    {
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0" ice="SRTED" ric="NRQRD" standard-event-code="PO">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="18.03.2016 09:02" event-text="Sorted" ice="LDTMV" ric="MVMTV" standard-event-code="AA"/>
                    </data>
                </data>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Status.ShouldBe(TrackingStatus.OutForDelivery);
    }

    [Fact]
    public async Task TrackAsync_PieceStatusAttribute_IsIgnoredForStatus()
    {
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" piece-status="5" delivery-event-flag="0">
                    <data name="piece-event-list"/>
                </data>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Status.ShouldBe(TrackingStatus.Unknown);
        result.Events.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("20.02.2026", 2026, 2, 20)]
    [InlineData("2026-02-20", 2026, 2, 20)]
    [InlineData("20.02.2026 14:30", 2026, 2, 20)]
    public async Task TrackAsync_DeliveryDate_IsParsedAsGermanLocalTime(string value, int year, int month, int day)
    {
        var xml = $"""
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0" standard-event-code="PO" delivery-date="{value}"/>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.EstimatedDelivery.ShouldNotBeNull();
        result.EstimatedDelivery!.Value.Date.ShouldBe(new DateTime(year, month, day));
        result.EstimatedDelivery.Value.Offset.ShouldBe(TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task TrackAsync_EventTimestamp_IsIndependentOfCurrentCulture()
    {
        // 03.04.2026 is 3 April in the DHL format; a US culture would read it as 4 March.
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0" standard-event-code="AA">
                    <data name="piece-event-list">
                        <data name="piece-event" event-timestamp="03.04.2026 10:00" event-text="Processed" standard-event-code="AA"/>
                    </data>
                </data>
            </data>
            """;
        var client = CreateClient(Xml(xml));

        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var result = await client.TrackAsync("12345");

            result.Events[0].Timestamp.ShouldBe(new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.FromHours(2)));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public async Task TrackAsync_EmptyEventList_ReturnsEmptyEvents()
    {
        var xml = """
            <data name="piece-shipment-list" code="0">
                <data name="piece-shipment" piece-code="12345" delivery-event-flag="0">
                    <data name="piece-event-list"/>
                </data>
            </data>
            """;

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Events.ShouldBeEmpty();
    }

    // --- get-status-for-public-user ---

    [Fact]
    public async Task TrackPublicAsync_DocumentationExample_ReadsShipmentElementNotListAttributes()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-status-public-delivered")));
        var client = CreateClient(handler);

        var result = await client.TrackPublicAsync("0231234");

        handler.LastRequest!.RequestUri!.ToString().ShouldContain("get-status-for-public-user");
        result.ShipmentNumber.ShouldBe("0231234");
        result.Status.ShouldBe(TrackingStatus.Delivered);
        result.Events.Count.ShouldBe(1);
        result.Events[0].Description.ShouldBe("Die Sendung wurde ausgeliefert.");
        result.Events[0].StatusCode.ShouldBe("ZU");
        result.Events[0].Location.ShouldBe("de");
        result.Events[0].Timestamp.ShouldBe(new DateTimeOffset(2012, 3, 11, 11, 59, 0, TimeSpan.FromHours(1)));
    }

    [Fact]
    public async Task TrackPublicAsync_OutForDelivery_MapsStatus()
    {
        var client = CreateClientWithFixture("piece-status-public-out-for-delivery");

        var result = await client.TrackPublicAsync("0231234");

        result.Status.ShouldBe(TrackingStatus.OutForDelivery);
    }

    [Fact]
    public async Task TrackPublicAsync_PieceStatusZeroOnSuccess_IsNotAnError()
    {
        // Public success responses carry piece-status="0" next to code="0".
        var client = CreateClientWithFixture("piece-status-public-delivered");

        await Should.NotThrowAsync(() => client.TrackPublicAsync("0231234"));
    }

    [Fact]
    public async Task TrackPublicAsync_PieceStatus100_ThrowsNotFoundException()
    {
        var client = CreateClientWithFixture("piece-status-public-not-found");

        var ex = await Should.ThrowAsync<DhlTrackingNotFoundException>(() => client.TrackPublicAsync("0231234"));

        ex.TrackingNumber.ShouldBe("0231234");
        ex.Message.ShouldContain("Keine Daten gefunden.");
    }

    [Fact]
    public async Task TrackPublicAsync_PieceStatus5_ThrowsTrackingExceptionThatIsNotNotFound()
    {
        var client = CreateClientWithFixture("piece-status-public-login-failed");

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackPublicAsync("0231234"));

        ex.ShouldNotBeOfType<DhlTrackingNotFoundException>();
        ex.ErrorCode.ShouldBe("5");
    }

    [Theory]
    [InlineData("""<data><data name="piece-status-public-list" code="0"><data name="piece-status-public" piece-status="100" piece-status-desc="Keine Daten gefunden."/></data></data>""", 100, true)]
    [InlineData("""<data><data name="piece-status-public-list" code="100"><data name="piece-status-public" piece-status="100" piece-status-desc="Keine Daten gefunden."/></data></data>""", 100, true)]
    [InlineData("""<data><data name="piece-status-public-list" code="0"><data name="piece-status-public" piece-status="5" piece-status-desc="Anmeldung fehlgeschlagen."/></data></data>""", 5, false)]
    public async Task TrackPublicAsync_PieceStatusErrorNextToListCode_IsReportedWithDescription(string xml, int code, bool notFound)
    {
        var client = CreateClient(Xml(xml));

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackPublicAsync("0231234"));

        ex.ErrorCode.ShouldBe(code.ToString());
        (ex is DhlTrackingNotFoundException).ShouldBe(notFound);
        ex.Message.ShouldNotContain("Unknown error");
    }

    [Fact]
    public async Task TrackAsync_EmptyRootCode_FallsBackToChildCode()
    {
        var xml = """<data code=""><data name="piece-shipment" code="0" piece-code="12345" delivery-event-flag="1"/></data>""";

        var result = await CreateClient(Xml(xml)).TrackAsync("12345");

        result.Status.ShouldBe(TrackingStatus.Delivered);
    }

    // --- Errors ---

    [Fact]
    public async Task TrackAsync_UnknownTrackingNumber_ThrowsNotFoundException()
    {
        var client = CreateClientWithFixture("not-found");

        var ex = await Should.ThrowAsync<DhlTrackingNotFoundException>(() => client.TrackAsync("00340434161094099999"));

        ex.TrackingNumber.ShouldBe("00340434161094099999");
        ex.ErrorCode.ShouldBe("100");
        ex.StatusCode.ShouldBeNull();
        ex.Message.ShouldContain("100");
        ex.Message.ShouldContain("Keine Daten gefunden.");
        ex.RawResponse.ShouldBe(Fixture("not-found"));
    }

    [Fact]
    public async Task TrackPublicAsync_UnknownTrackingNumber_ThrowsNotFoundException()
    {
        var client = CreateClientWithFixture("not-found");

        var ex = await Should.ThrowAsync<DhlTrackingNotFoundException>(() => client.TrackPublicAsync("00340434161094099999"));

        ex.TrackingNumber.ShouldBe("00340434161094099999");
    }

    [Fact]
    public async Task TrackAsync_NotFound_CanBeCaughtAsTrackingException()
    {
        var client = CreateClientWithFixture("not-found");

        await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("00340434161094099999"));
    }

    [Fact]
    public async Task TrackAsync_LoginFailed_ThrowsTrackingExceptionThatIsNotNotFound()
    {
        var client = CreateClientWithFixture("login-failed");

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("00340434161094042557"));

        ex.ShouldNotBeOfType<DhlTrackingNotFoundException>();
        ex.ErrorCode.ShouldBe("5");
        ex.StatusCode.ShouldBeNull();
        ex.Message.ShouldContain("Anmeldung fehlgeschlagen.");
        ex.RawResponse.ShouldBe(Fixture("login-failed"));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(62)]
    [InlineData(200)]
    public async Task TrackAsync_OtherErrorCodes_ThrowTrackingExceptionWithCode(int code)
    {
        var xml = $"""<data name="piece-shipment-list" code="{code}" error="Fehler"><data name="piece-shipment" piece-status="{code}"/></data>""";
        var client = CreateClient(Xml(xml));

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));

        ex.ShouldNotBeOfType<DhlTrackingNotFoundException>();
        ex.ErrorCode.ShouldBe(code.ToString());
    }

    [Fact]
    public async Task TrackAsync_ResponseWithoutCode_ThrowsTrackingException()
    {
        var client = CreateClient(Xml("<data name=\"piece-shipment-list\"/>"));

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));

        ex.ErrorCode.ShouldBe("-1");
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
    public async Task TrackAsync_HttpError_DoesNotLeakBodyIntoMessage()
    {
        var body = "<error>Max Mustermann, Musterstrasse 1, 12345 Musterstadt</error>";
        var client = CreateClient(Xml(body, HttpStatusCode.InternalServerError));

        var ex = await Should.ThrowAsync<TrackingException>(() => client.TrackAsync("12345"));
        ex.Message.ShouldBe("DHL Tracking API returned 500 without error details.");
        ex.Message.ShouldNotContain("Mustermann");
        ex.RawResponse.ShouldBe(body);
    }

    [Fact]
    public async Task GetSignatureAsync_HttpError_DoesNotLeakBodyIntoMessage()
    {
        var body = "<error>Max Mustermann, Musterstrasse 1, 12345 Musterstadt</error>";
        var client = CreateClient(Xml(body, HttpStatusCode.InternalServerError));

        var ex = await Should.ThrowAsync<TrackingException>(() => client.GetSignatureAsync("12345"));
        ex.Message.ShouldBe("DHL Tracking API returned 500 without error details.");
        ex.Message.ShouldNotContain("Mustermann");
        ex.RawResponse.ShouldBe(body);
    }

    [Fact]
    public async Task TrackAsync_NullTrackingNumber_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.TrackAsync(null!));
    }

    // --- Authentication ---

    [Fact]
    public async Task TrackAsync_SendsBasicAuthorizationWithApiKeyAndSecret()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));

        await CreateClient(handler).TrackAsync("00340434161094042557");

        AssertBasicAuthorization(handler.LastRequest);
    }

    [Fact]
    public async Task TrackPublicAsync_SendsBasicAuthorizationWithApiKeyAndSecret()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-status-public-delivered")));

        await CreateClient(handler).TrackPublicAsync("0231234");

        AssertBasicAuthorization(handler.LastRequest);
    }

    [Fact]
    public async Task GetSignatureAsync_SendsBasicAuthorizationWithApiKeyAndSecret()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("signature")));

        await CreateClient(handler).GetSignatureAsync("00340434161094042557");

        AssertBasicAuthorization(handler.LastRequest);
    }

    [Fact]
    public async Task TrackAsync_BasicAuthorization_UsesUtf8()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));
        var options = CreateDhlOptions("geheim-ü€");

        await CreateClient(handler, options).TrackAsync("00340434161094042557");

        handler.LastRequest!.Headers.Authorization!.Parameter.ShouldBe(ExpectedBasicCredentials(ApiKey, "geheim-ü€"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task TrackAsync_WithoutApiSecret_ThrowsInvalidOperationExceptionBeforeSending(string? secret)
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));
        var client = CreateClient(handler, CreateDhlOptions(secret));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => client.TrackAsync("00340434161094042557"));

        ex.Message.ShouldContain("ApiSecret");
        handler.RequestCount.ShouldBe(0);
    }

    [Fact]
    public async Task TrackPublicAsync_WithoutApiSecret_ThrowsInvalidOperationExceptionBeforeSending()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-status-public-delivered")));
        var client = CreateClient(handler, CreateDhlOptions(null));

        await Should.ThrowAsync<InvalidOperationException>(() => client.TrackPublicAsync("0231234"));

        handler.RequestCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetSignatureAsync_WithoutApiSecret_ThrowsInvalidOperationException()
    {
        var client = CreateClient(Xml(Fixture("signature")), CreateDhlOptions(null));

        await Should.ThrowAsync<InvalidOperationException>(() => client.GetSignatureAsync("00340434161094042557"));
    }

    private static void AssertBasicAuthorization(HttpRequestMessage? request)
    {
        request.ShouldNotBeNull();
        request.Headers.Authorization.ShouldNotBeNull();
        request.Headers.Authorization!.Scheme.ShouldBe("Basic");
        request.Headers.Authorization.Parameter.ShouldBe(ExpectedBasicCredentials());
    }

    // --- Request building ---

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
    public void BuildXmlQuery_XmlInjectionAttempt_IsEscaped()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        var xml = client.BuildXmlQuery("d-get-piece-detail", """12345" malicious="true""", null);

        xml.ShouldNotContain("malicious=\"true\"");
        xml.ShouldContain("piece-code=");
        System.Xml.Linq.XElement.Parse(xml);
    }

    [Fact]
    public async Task TrackAsync_RequestUrl_ContainsXmlQueryParameter()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));

        await CreateClient(handler).TrackAsync("00340434161094042557");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.GetLeftPart(UriPartial.Path).ShouldBe(SandboxUrl);
        handler.LastRequest.RequestUri.Query.ShouldContain("xml=");
        handler.LastRequest.RequestUri.Query.ShouldContain("d-get-piece-detail");
    }

    // --- d-get-signature ---

    [Fact]
    public async Task GetSignatureAsync_ReturnsBytes_FromHexEncoding()
    {
        var client = CreateClientWithFixture("signature");

        var bytes = await client.GetSignatureAsync("00340434161094042557");

        bytes.ShouldNotBeNull();
        bytes!.Take(6).ToArray().ShouldBe("GIF89a"u8.ToArray());
        bytes.Length.ShouldBe(Convert.FromHexString("47494638396101000100800000000000FFFFFF21F90401000000002C00000000010001000002024401003B").Length);
    }

    [Fact]
    public async Task GetSignatureAsync_NoDataFound_ReturnsNull()
    {
        var client = CreateClientWithFixture("not-found");

        var bytes = await client.GetSignatureAsync("00340434161094099999");

        bytes.ShouldBeNull();
    }

    [Fact]
    public async Task GetSignatureAsync_LoginFailed_ThrowsTrackingException()
    {
        var client = CreateClientWithFixture("login-failed");

        var ex = await Should.ThrowAsync<TrackingException>(() => client.GetSignatureAsync("00340434161094042557"));

        ex.ErrorCode.ShouldBe("5");
        ex.ShouldNotBeOfType<DhlTrackingNotFoundException>();
    }

    [Fact]
    public async Task GetSignatureAsync_ArchiveUnavailable_ThrowsTrackingException()
    {
        var client = CreateClientWithFixture("signature-unavailable");

        var ex = await Should.ThrowAsync<TrackingException>(() => client.GetSignatureAsync("00340434161094042557"));

        ex.ErrorCode.ShouldBe("63");
    }

    [Fact]
    public async Task GetSignatureAsync_SuccessWithoutImage_ReturnsNull()
    {
        var client = CreateClient(Xml("""<data name="signature-list" code="0"/>"""));

        var bytes = await client.GetSignatureAsync("00340434161094042557");

        bytes.ShouldBeNull();
    }
}
