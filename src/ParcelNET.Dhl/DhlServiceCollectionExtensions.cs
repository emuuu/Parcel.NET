using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl;

/// <summary>
/// Extension methods for registering DHL services with the dependency injection container.
/// </summary>
public static class DhlServiceCollectionExtensions
{
    /// <summary>
    /// Adds DHL core services including configuration, authentication handlers, and token management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="DhlOptions"/>.</param>
    /// <returns>A <see cref="DhlBuilder"/> for chaining DHL sub-service registrations.</returns>
    public static DhlBuilder AddDhl(this IServiceCollection services, Action<DhlOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Configure(configure)
            .Validate(o => !string.IsNullOrEmpty(o.ApiKey), "DHL ApiKey is required.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomShippingBaseUrl), "CustomShippingBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomTrackingBaseUrl), "CustomTrackingBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomUnifiedTrackingBaseUrl), "CustomUnifiedTrackingBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomPickupBaseUrl), "CustomPickupBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomReturnsBaseUrl), "CustomReturnsBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomInternetmarkeBaseUrl), "CustomInternetmarkeBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomLocationFinderBaseUrl), "CustomLocationFinderBaseUrl must use HTTPS.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomTokenUrl), "CustomTokenUrl must use HTTPS.");

        services.AddHttpClient(DhlTokenService.TokenHttpClientName);
        services.AddSingleton<IDhlTokenService, DhlTokenService>();
        services.AddTransient<DhlAuthHandler>();
        services.AddTransient<DhlApiKeyHandler>();

        return new DhlBuilder(services);
    }

    private static bool IsHttpsOrLocalhost(string? url)
    {
        if (url is null) return true;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme == Uri.UriSchemeHttps) return true;

        var host = uri.Host;
        return host is "localhost" or "127.0.0.1" or "::1";
    }
}
