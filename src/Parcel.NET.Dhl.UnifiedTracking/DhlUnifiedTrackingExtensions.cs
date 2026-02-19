using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions;

namespace Parcel.NET.Dhl.UnifiedTracking;

/// <summary>
/// Extension methods for registering the DHL Unified Tracking client.
/// </summary>
public static class DhlUnifiedTrackingExtensions
{
    /// <summary>
    /// Adds the DHL Unified Tracking client (JSON API) to the service collection.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlUnifiedTracking(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<DhlUnifiedTrackingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.UnifiedTrackingBaseUrl);
        })
        .AddHttpMessageHandler<DhlApiKeyHandler>()
        .RedactLoggedHeaders(["dhl-api-key"]);

        builder.Services.AddTransient<IDhlUnifiedTrackingClient>(sp => sp.GetRequiredService<DhlUnifiedTrackingClient>());

        return builder;
    }
}
