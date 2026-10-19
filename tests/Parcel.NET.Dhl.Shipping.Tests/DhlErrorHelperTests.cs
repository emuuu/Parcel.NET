using System.Net;
using Parcel.NET.Dhl.Internal;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Shipping.Tests;

public class DhlErrorHelperTests
{
    [Fact]
    public void FormatErrorMessage_StatusDetail_UsesDetail()
    {
        var message = DhlErrorHelper.FormatErrorMessage(
            "DHL Test API", HttpStatusCode.BadRequest,
            """{"status":{"detail":"Invalid billing number."},"detail":"other"}""");

        message.ShouldBe("DHL Test API returned 400: Invalid billing number.");
    }

    [Fact]
    public void FormatErrorMessage_TopLevelDetail_UsesDetail()
    {
        var message = DhlErrorHelper.FormatErrorMessage(
            "DHL Test API", HttpStatusCode.NotFound, """{"detail":"Shipment not found."}""");

        message.ShouldBe("DHL Test API returned 404: Shipment not found.");
    }

    [Fact]
    public void FormatErrorMessage_NotJson_DoesNotContainBody()
    {
        var body = "Max Mustermann, Musterstraße 1";

        var message = DhlErrorHelper.FormatErrorMessage("DHL Test API", HttpStatusCode.BadRequest, body);

        message.ShouldBe("DHL Test API returned 400 without error details.");
        message.ShouldNotContain("Mustermann");
    }

    [Fact]
    public void FormatErrorMessage_JsonWithoutDetail_DoesNotContainBody()
    {
        var body = """{"status":{"title":"Bad Request"},"consignee":{"name1":"Max Mustermann"}}""";

        var message = DhlErrorHelper.FormatErrorMessage("DHL Test API", HttpStatusCode.BadRequest, body);

        message.ShouldBe("DHL Test API returned 400 without error details.");
        message.ShouldNotContain("Mustermann");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("""{"detail":42}""")]
    [InlineData("""{"detail":""}""")]
    [InlineData("""{"status":{"detail":null}}""")]
    [InlineData("""{"status":"Bad Request"}""")]
    [InlineData("""{"detail":"\uD800"}""")]
    [InlineData("""{"status":{"detail":"\uD800"}}""")]
    public void TryGetErrorDetail_UnusableBody_ReturnsNull(string? body)
    {
        DhlErrorHelper.TryGetErrorDetail(body).ShouldBeNull();
    }

#pragma warning disable CS0618 // Obsolete compatibility shim is tested on purpose
    [Fact]
    public void TryParseErrorDetail_CompatibilityShim_ReturnsDetail()
    {
        DhlErrorHelper.TryParseErrorDetail("""{"detail":"Order not found."}""").ShouldBe("Order not found.");
    }

    [Fact]
    public void TryParseErrorDetail_CompatibilityShim_NeverReturnsBody()
    {
        var body = "Max Mustermann, Musterstraße 1";

        DhlErrorHelper.TryParseErrorDetail(body).ShouldBe("DHL API request failed.");
    }
#pragma warning restore CS0618
}
