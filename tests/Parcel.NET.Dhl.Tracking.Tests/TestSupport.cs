using System.Net;
using System.Text;
using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl.Tracking.Tests;

internal static class TestSupport
{
    internal const string ApiKey = "test-api-key";
    internal const string ApiSecret = "test-api-secret";
    internal const string SandboxUrl = "https://api-sandbox.dhl.com/parcel/de/tracking/v0/shipments";

    internal static DhlOptions CreateDhlOptions(string? apiSecret = ApiSecret) => new()
    {
        ApiKey = ApiKey,
        ApiSecret = apiSecret,
        TrackingUsername = "zt12345",
        TrackingPassword = "geheim",
        UseSandbox = true
    };

    internal static string Fixture(string name)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".xml"), Encoding.UTF8);

    internal static HttpResponseMessage Xml(string xml, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(xml, Encoding.UTF8, "application/xml") };

    internal static DhlTrackingClient CreateClient(HttpMessageHandler handler, DhlOptions? options = null)
        => new(
            new HttpClient(handler) { BaseAddress = new Uri(SandboxUrl) },
            Microsoft.Extensions.Options.Options.Create(options ?? CreateDhlOptions()));

    internal static DhlTrackingClient CreateClient(HttpResponseMessage response, DhlOptions? options = null)
        => CreateClient(new MockHttpMessageHandler(response), options);

    internal static DhlTrackingClient CreateClientWithFixture(string fixture)
        => CreateClient(Xml(Fixture(fixture)));

    internal static string ExpectedBasicCredentials(string key = ApiKey, string secret = ApiSecret)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{key}:{secret}"));
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    public HttpRequestMessage? LastRequest { get; private set; }
    public int RequestCount { get; private set; }

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        RequestCount++;
        return Task.FromResult(_response);
    }
}
