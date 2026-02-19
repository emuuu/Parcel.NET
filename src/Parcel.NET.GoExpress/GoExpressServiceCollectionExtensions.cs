using Microsoft.Extensions.DependencyInjection;

namespace Parcel.NET.GoExpress;

/// <summary>
/// Extension methods for registering GO! Express services with the dependency injection container.
/// </summary>
public static class GoExpressServiceCollectionExtensions
{
    /// <summary>
    /// Adds GO! Express core services including configuration and authentication handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="GoExpressOptions"/>.</param>
    /// <returns>A <see cref="GoExpressBuilder"/> for chaining GO! Express sub-service registrations.</returns>
    public static GoExpressBuilder AddGoExpress(this IServiceCollection services, Action<GoExpressOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptionsWithValidateOnStart<GoExpressOptions>()
            .Configure(configure)
            .Validate(o => !string.IsNullOrEmpty(o.Username), "GO! Express Username is required.")
            .Validate(o => !string.IsNullOrEmpty(o.Password), "GO! Express Password is required.")
            .Validate(o => !string.IsNullOrEmpty(o.CustomerId), "GO! Express CustomerId is required.")
            .Validate(o => o.CustomerId.Length <= 7, "GO! Express CustomerId must be at most 7 characters.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomBaseUrl), "CustomBaseUrl must use HTTPS.");

        services.AddTransient<GoExpressBasicAuthHandler>();

        return new GoExpressBuilder(services);
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
