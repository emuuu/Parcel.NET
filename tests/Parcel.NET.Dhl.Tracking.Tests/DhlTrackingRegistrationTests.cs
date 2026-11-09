using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions.Models;
using Shouldly;
using Xunit;
using static Parcel.NET.Dhl.Tracking.Tests.TestSupport;

namespace Parcel.NET.Dhl.Tracking.Tests;

public class DhlTrackingRegistrationTests
{
    private static ServiceCollection CreateServices(Action<DhlOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddDhl(o =>
        {
            o.ApiKey = ApiKey;
            o.ApiSecret = ApiSecret;
            o.TrackingUsername = "zt12345";
            o.TrackingPassword = "geheim";
            o.UseSandbox = true;
            configure?.Invoke(o);
        }).AddDhlTracking();
        return services;
    }

    [Fact]
    public void AddDhlTracking_WithoutApiSecret_FailsValidation()
    {
        var services = CreateServices(o => o.ApiSecret = null);
        using var provider = services.BuildServiceProvider();

        var ex = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DhlOptions>>().Value);

        ex.Message.ShouldContain("DHL ApiSecret is required for Parcel DE Tracking");
    }

    [Fact]
    public void AddDhlTracking_WithEmptyApiSecret_FailsValidation()
    {
        var services = CreateServices(o => o.ApiSecret = "");
        using var provider = services.BuildServiceProvider();

        Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DhlOptions>>().Value);
    }

    [Fact]
    public void AddDhlTracking_WithApiSecret_PassesValidation()
    {
        using var provider = CreateServices().BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<DhlOptions>>().Value;

        options.ApiSecret.ShouldBe(ApiSecret);
    }

    [Fact]
    public void AddDhlTracking_WithoutTrackingCredentials_FailsValidation()
    {
        using var provider = CreateServices(o => o.TrackingPassword = null).BuildServiceProvider();

        var ex = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DhlOptions>>().Value);

        ex.Message.ShouldContain("TrackingPassword");
    }

    [Fact]
    public async Task AddDhlTracking_Pipeline_SendsApiKeyHeaderAndBasicAuthorization()
    {
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));
        var services = CreateServices();
        services.AddHttpClient<DhlTrackingClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IDhlTrackingClient>();
        var result = await client.TrackAsync("00340434161094042557");

        result.Status.ShouldBe(TrackingStatus.Delivered);
        handler.LastRequest!.Headers.GetValues("dhl-api-key").ShouldBe([ApiKey]);
        handler.LastRequest.Headers.Authorization!.Scheme.ShouldBe("Basic");
        handler.LastRequest.Headers.Authorization.Parameter.ShouldBe(ExpectedBasicCredentials());
    }

    [Fact]
    public async Task AddDhlTracking_DefaultLogging_RedactsQueryAndCredentialHeaders()
    {
        var logs = new CapturingLoggerProvider();
        var handler = new MockHttpMessageHandler(Xml(Fixture("piece-detail-delivered")));
        var services = CreateServices();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace).AddProvider(logs));
        services.AddHttpClient<DhlTrackingClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<IDhlTrackingClient>().TrackAsync("00340434161094042557");

        var all = string.Join(Environment.NewLine, logs.Entries);
        logs.Entries.ShouldNotBeEmpty();

        // The default HttpClientFactory logging redacts the query string ("?*"), which carries the XML
        // request with appname/password, and the registration redacts both credential headers.
        var requestLines = logs.Entries.Where(e => e.Contains("HTTP request GET")).ToList();
        requestLines.ShouldNotBeEmpty();
        requestLines.ShouldAllBe(e => e.Contains(SandboxUrl + "?*"));
        all.ShouldNotContain("xml=");
        all.ShouldNotContain("piece-code");
        all.ShouldNotContain("geheim");
        all.ShouldNotContain("zt12345");
        all.ShouldNotContain(ApiSecret);
        all.ShouldNotContain(ApiKey);
        all.ShouldNotContain(ExpectedBasicCredentials());
        all.ShouldNotContain("Kraemer");
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<string> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, Entries);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, List<string> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var text = $"{logLevel} {category}: {formatter(state, exception)}";
                if (state is IEnumerable<KeyValuePair<string, object?>> values)
                    text += " | " + string.Join("; ", values.Select(v => $"{v.Key}={v.Value}"));
                lock (entries) entries.Add(text);
            }
        }
    }
}
