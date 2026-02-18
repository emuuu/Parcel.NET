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
            .Validate(o => !string.IsNullOrEmpty(o.ApiKey), "DHL ApiKey is required.");

        services.AddHttpClient(DhlTokenService.TokenHttpClientName);
        services.AddSingleton<IDhlTokenService, DhlTokenService>();
        services.AddTransient<DhlAuthHandler>();
        services.AddTransient<DhlApiKeyHandler>();

        return new DhlBuilder(services);
    }
}
