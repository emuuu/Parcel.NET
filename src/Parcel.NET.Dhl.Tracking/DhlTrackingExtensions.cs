using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions;

namespace Parcel.NET.Dhl.Tracking;

/// <summary>
/// Extension methods for registering the DHL Parcel DE Tracking client (XML API v0).
/// </summary>
public static class DhlTrackingExtensions
{
    /// <summary>
    /// Adds the DHL Parcel DE Tracking client (XML API) to the service collection.
    /// Requires <see cref="DhlOptions.ApiKey"/>, <see cref="DhlOptions.ApiSecret"/> (Basic authentication of the DHL app),
    /// <see cref="DhlOptions.TrackingUsername"/> and <see cref="DhlOptions.TrackingPassword"/> (business customer portal user,
    /// sent inside the XML query) to be set.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlTracking(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.ApiSecret), "DHL ApiSecret is required for Parcel DE Tracking (Basic authentication with ApiKey:ApiSecret).")
            .Validate(o => !string.IsNullOrEmpty(o.TrackingUsername), "DHL TrackingUsername is required for Parcel DE Tracking.")
            .Validate(o => !string.IsNullOrEmpty(o.TrackingPassword), "DHL TrackingPassword is required for Parcel DE Tracking.");

        builder.Services.AddHttpClient<DhlTrackingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.TrackingBaseUrl);
        })
        .AddHttpMessageHandler<DhlApiKeyHandler>()
        .RedactLoggedHeaders(["dhl-api-key", "Authorization"]);

        builder.Services.AddTransient<IDhlTrackingClient>(sp => sp.GetRequiredService<DhlTrackingClient>());

        return builder;
    }
}
