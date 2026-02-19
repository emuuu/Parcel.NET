using System.Diagnostics;
using Microsoft.Extensions.Options;
using ParcelNET.Abstractions.Models;
using ParcelNET.Dhl;
using ParcelNET.Dhl.Shipping;
using ParcelNET.Dhl.Shipping.Models;
using ParcelNET.Dhl.UnifiedTracking;
using ParcelNET.Docs.Playground.Models;
using ParcelNET.GoExpress;
using ParcelNET.GoExpress.Shipping;
using ParcelNET.GoExpress.Shipping.Models;

namespace ParcelNET.Docs.Playground.Services;

public class CarrierClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CarrierClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PlaygroundResult> ExecuteDhlShippingAsync(
        DhlCredentials credentials,
        Func<DhlShippingClient, Task<object>> operation)
    {
        var opts = new DhlOptions
        {
            ApiKey = credentials.ApiKey,
            ApiSecret = credentials.ApiSecret,
            Username = credentials.Username,
            Password = credentials.Password,
            UseSandbox = true
        };

        var tokenService = new DhlTokenService(Options.Create(opts), _httpClientFactory);
        try
        {
            var authHandler = new DhlAuthHandler(Options.Create(opts), tokenService)
            {
                InnerHandler = new HttpClientHandler()
            };

            using var httpClient = new HttpClient(authHandler)
            {
                BaseAddress = new Uri(opts.ShippingBaseUrl)
            };

            var client = new DhlShippingClient(httpClient);

            var sw = Stopwatch.StartNew();
            try
            {
                var result = await operation(client);
                sw.Stop();
                return PlaygroundResult.Success(result, sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return PlaygroundResult.Error(ex, sw.Elapsed);
            }
        }
        finally
        {
            tokenService.Dispose();
        }
    }

    public async Task<PlaygroundResult> ExecuteDhlTrackingAsync(
        DhlCredentials credentials,
        Func<DhlUnifiedTrackingClient, Task<object>> operation)
    {
        var opts = new DhlOptions
        {
            ApiKey = credentials.ApiKey,
            UseSandbox = true
        };

        var apiKeyHandler = new DhlApiKeyHandler(Options.Create(opts))
        {
            InnerHandler = new HttpClientHandler()
        };

        using var httpClient = new HttpClient(apiKeyHandler)
        {
            BaseAddress = new Uri(opts.UnifiedTrackingBaseUrl)
        };

        var client = new DhlUnifiedTrackingClient(httpClient);

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await operation(client);
            sw.Stop();
            return PlaygroundResult.Success(result, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return PlaygroundResult.Error(ex, sw.Elapsed);
        }
    }

    public async Task<PlaygroundResult> ExecuteGoExpressShippingAsync(
        GoExpressCredentials credentials,
        Func<GoExpressShippingClient, Task<object>> operation)
    {
        var opts = new GoExpressOptions
        {
            Username = credentials.Username,
            Password = credentials.Password,
            CustomerId = credentials.CustomerId,
            ResponsibleStation = string.IsNullOrWhiteSpace(credentials.ResponsibleStation)
                ? null
                : credentials.ResponsibleStation,
            UseSandbox = true
        };

        var authHandler = new GoExpressBasicAuthHandler(Options.Create(opts))
        {
            InnerHandler = new HttpClientHandler()
        };

        using var httpClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri(opts.BaseUrl)
        };

        var client = new GoExpressShippingClient(httpClient, Options.Create(opts));

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await operation(client);
            sw.Stop();
            return PlaygroundResult.Success(result, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return PlaygroundResult.Error(ex, sw.Elapsed);
        }
    }
}
