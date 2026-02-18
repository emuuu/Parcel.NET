using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ParcelNET.Dhl.Shipping.Tests;

public class DhlTokenServiceTests
{
    private static DhlOptions CreateOptions() => new()
    {
        ApiKey = "test-api-key",
        ApiSecret = "test-api-secret",
        Username = "test-user",
        Password = "test-pass"
    };

    private static IOptions<DhlOptions> WrapOptions(DhlOptions options)
    {
        var wrapper = Substitute.For<IOptions<DhlOptions>>();
        wrapper.Value.Returns(options);
        return wrapper;
    }

    private static IHttpClientFactory CreateFactory(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(DhlTokenService.TokenHttpClientName).Returns(httpClient);
        return factory;
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsToken()
    {
        var tokenResponse = new { access_token = "test-token-123", expires_in = 3600, token_type = "bearer" };
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(tokenResponse)
        });

        var service = new DhlTokenService(WrapOptions(CreateOptions()), factory);

        var token = await service.GetAccessTokenAsync();

        token.ShouldBe("test-token-123");
    }

    [Fact]
    public async Task GetAccessTokenAsync_CachesToken()
    {
        var tokenResponse = new { access_token = "cached-token", expires_in = 3600, token_type = "bearer" };
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(tokenResponse)
        });

        var service = new DhlTokenService(WrapOptions(CreateOptions()), factory);

        var token1 = await service.GetAccessTokenAsync();
        var token2 = await service.GetAccessTokenAsync();

        token1.ShouldBe(token2);
        // Factory should only be called once (second call uses cache)
        factory.Received(1).CreateClient(DhlTokenService.TokenHttpClientName);
    }

    [Fact]
    public async Task GetAccessTokenAsync_MissingUsername_Throws()
    {
        var options = new DhlOptions { ApiKey = "key", ApiSecret = "secret" };
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK));

        var service = new DhlTokenService(WrapOptions(options), factory);

        await Should.ThrowAsync<InvalidOperationException>(() => service.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_MissingApiSecret_Throws()
    {
        var options = new DhlOptions { ApiKey = "key", Username = "user", Password = "pass" };
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK));

        var service = new DhlTokenService(WrapOptions(options), factory);

        await Should.ThrowAsync<InvalidOperationException>(() => service.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_HttpError_Throws()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var service = new DhlTokenService(WrapOptions(CreateOptions()), factory);

        await Should.ThrowAsync<HttpRequestException>(() => service.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_ConcurrentAccess_OnlyOneTokenRequest()
    {
        var tokenResponse = new { access_token = "concurrent-token", expires_in = 3600, token_type = "bearer" };
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(tokenResponse)
        });

        var service = new DhlTokenService(WrapOptions(CreateOptions()), factory);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetAccessTokenAsync())
            .ToArray();

        var tokens = await Task.WhenAll(tasks);

        tokens.ShouldAllBe(t => t == "concurrent-token");
    }
}
