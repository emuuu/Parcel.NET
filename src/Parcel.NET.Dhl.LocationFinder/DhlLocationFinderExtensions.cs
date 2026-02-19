using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl.LocationFinder;

/// <summary>
/// Extension methods for registering the DHL Location Finder client.
/// </summary>
public static class DhlLocationFinderExtensions
{
    /// <summary>
    /// Adds the DHL Location Finder client (API-Key auth) to the service collection.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlLocationFinder(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<DhlLocationFinderClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.LocationFinderBaseUrl);
        })
        .AddHttpMessageHandler<DhlApiKeyHandler>()
        .RedactLoggedHeaders(["dhl-api-key"]);

        builder.Services.AddTransient<IDhlLocationFinderClient>(sp => sp.GetRequiredService<DhlLocationFinderClient>());

        return builder;
    }
}
