using Microsoft.Extensions.DependencyInjection;

namespace Parcel.NET.LetterXpress;

/// <summary>
/// Extension methods for registering LetterXpress services with the dependency injection container.
/// </summary>
public static class LetterXpressServiceCollectionExtensions
{
    /// <summary>
    /// Adds LetterXpress core services including configuration validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="LetterXpressOptions"/>.</param>
    /// <returns>A <see cref="LetterXpressBuilder"/> for chaining LetterXpress sub-service registrations.</returns>
    public static LetterXpressBuilder AddLetterXpress(this IServiceCollection services, Action<LetterXpressOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptionsWithValidateOnStart<LetterXpressOptions>()
            .Configure(configure)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username), "LetterXpress Username is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "LetterXpress ApiKey is required.")
            .Validate(o => IsHttpsOrLocalhost(o.CustomBaseUrl), "CustomBaseUrl must use HTTPS.");

        return new LetterXpressBuilder(services);
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
