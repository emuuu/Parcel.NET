using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions;

namespace Parcel.NET.GoExpress.Tracking;

/// <summary>
/// Extension methods for registering the GO! Express tracking client.
/// </summary>
public static class GoExpressTrackingExtensions
{
    /// <summary>
    /// Adds the GO! Express tracking client to the service collection.
    /// </summary>
    /// <param name="builder">The GO! Express builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static GoExpressBuilder AddGoExpressTracking(this GoExpressBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<ITrackingService, GoExpressTrackingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GoExpressOptions>>().Value;
            client.BaseAddress = new Uri(options.TrackingBaseUrl);
        })
        .AddHttpMessageHandler<GoExpressBasicAuthHandler>()
        .RedactLoggedHeaders(["Authorization"]);

        return builder;
    }
}
