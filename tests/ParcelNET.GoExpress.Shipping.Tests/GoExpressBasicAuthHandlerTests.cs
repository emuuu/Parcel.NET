using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace ParcelNET.GoExpress.Shipping.Tests;

public class GoExpressBasicAuthHandlerTests
{
    [Fact]
    public async Task SendAsync_SetsBasicAuthHeader()
    {
        var options = Options.Create(new GoExpressOptions
        {
            Username = "testuser",
            Password = "testpass",
            CustomerId = "CUST01"
        });

        var innerHandler = new MockInnerHandler();
        var handler = new GoExpressBasicAuthHandler(options) { InnerHandler = innerHandler };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        await client.GetAsync("/test");

        innerHandler.LastRequest.ShouldNotBeNull();
        innerHandler.LastRequest.Headers.Authorization.ShouldNotBeNull();
        innerHandler.LastRequest.Headers.Authorization.Scheme.ShouldBe("Basic");
    }

    [Fact]
    public async Task SendAsync_CorrectBase64Encoding()
    {
        var options = Options.Create(new GoExpressOptions
        {
            Username = "myuser",
            Password = "myp@ss",
            CustomerId = "CUST01"
        });

        var innerHandler = new MockInnerHandler();
        var handler = new GoExpressBasicAuthHandler(options) { InnerHandler = innerHandler };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        await client.GetAsync("/test");

        var expectedEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("myuser:myp@ss"));
        innerHandler.LastRequest!.Headers.Authorization!.Parameter.ShouldBe(expectedEncoded);
    }

    [Fact]
    public async Task SendAsync_CachesCredentialsAcrossRequests()
    {
        var options = Options.Create(new GoExpressOptions
        {
            Username = "testuser",
            Password = "testpass",
            CustomerId = "CUST01"
        });

        var innerHandler = new MockInnerHandler();
        var handler = new GoExpressBasicAuthHandler(options) { InnerHandler = innerHandler };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        await client.GetAsync("/first");
        var firstCredentials = innerHandler.LastRequest!.Headers.Authorization!.Parameter;

        await client.GetAsync("/second");
        var secondCredentials = innerHandler.LastRequest!.Headers.Authorization!.Parameter;

        firstCredentials.ShouldBe(secondCredentials);
    }
}

internal class MockInnerHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
