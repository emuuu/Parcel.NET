using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ParcelNET.Dhl.Shipping.Tests;

public class DhlAuthHandlerTests
{
    private static IOptions<DhlOptions> CreateOptions()
    {
        var options = Substitute.For<IOptions<DhlOptions>>();
        options.Value.Returns(new DhlOptions
        {
            ApiKey = "test-api-key",
            ApiSecret = "test-secret"
        });
        return options;
    }

    [Fact]
    public async Task SendAsync_DoesNotAddApiKeyHeader()
    {
        var tokenService = Substitute.For<IDhlTokenService>();
        tokenService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("test-token");

        var innerHandler = new MockInnerHandler();
        var handler = new DhlAuthHandler(CreateOptions(), tokenService)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api-eu.dhl.com") };
        await client.GetAsync("/parcel/de/shipping/v2/orders");

        innerHandler.LastRequest!.Headers.Contains("dhl-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task SendAsync_AddsBearerToken()
    {
        var tokenService = Substitute.For<IDhlTokenService>();
        tokenService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("my-bearer-token");

        var innerHandler = new MockInnerHandler();
        var handler = new DhlAuthHandler(CreateOptions(), tokenService)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api-eu.dhl.com") };
        await client.GetAsync("/parcel/de/shipping/v2/orders");

        innerHandler.LastRequest!.Headers.Authorization.ShouldNotBeNull();
        innerHandler.LastRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        innerHandler.LastRequest.Headers.Authorization.Parameter.ShouldBe("my-bearer-token");
    }
}

public class DhlApiKeyHandlerTests
{
    [Fact]
    public async Task SendAsync_AddsOnlyApiKeyHeader()
    {
        var options = Substitute.For<IOptions<DhlOptions>>();
        options.Value.Returns(new DhlOptions { ApiKey = "tracking-key" });

        var innerHandler = new MockInnerHandler();
        var handler = new DhlApiKeyHandler(options)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api-eu.dhl.com") };
        await client.GetAsync("/track/shipments?trackingNumber=123");

        innerHandler.LastRequest!.Headers.GetValues("dhl-api-key").ShouldContain("tracking-key");
        innerHandler.LastRequest.Headers.Authorization.ShouldBeNull();
    }
}

internal class MockInnerHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}
