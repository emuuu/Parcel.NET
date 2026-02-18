using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParcelNET.Abstractions;

namespace ParcelNET.Dhl.Tracking;

/// <summary>
/// Extension methods for registering the DHL tracking client.
/// </summary>
public static class DhlTrackingExtensions
{
    /// <summary>
    /// Adds the DHL tracking client to the service collection.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlTracking(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<ITrackingService, DhlTrackingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.TrackingBaseUrl);
        })
        .AddHttpMessageHandler<DhlApiKeyHandler>()
        .RedactLoggedHeaders(["dhl-api-key", "Authorization"]);

        return builder;
    }
}
