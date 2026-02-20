using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl.Internetmarke;

/// <summary>
/// Extension methods for registering the DHL Internetmarke client.
/// </summary>
public static class DhlInternetmarkeExtensions
{
    /// <summary>
    /// Adds the DHL Post DE Internetmarke client to the service collection.
    /// Uses a custom Internetmarke-specific auth handler that calls POST /user with client_credentials grant type,
    /// separate from the shared DHL ROPC token endpoint.
    /// Requires <see cref="DhlOptions.InternetmarkeUsername"/>/<see cref="DhlOptions.InternetmarkePassword"/>
    /// (or falls back to <see cref="DhlOptions.Username"/>/<see cref="DhlOptions.Password"/>).
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlInternetmarke(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.InternetmarkeUsername ?? o.Username),
                "DHL Username or InternetmarkeUsername is required for Internetmarke operations.")
            .Validate(o => !string.IsNullOrEmpty(o.InternetmarkePassword ?? o.Password),
                "DHL Password or InternetmarkePassword is required for Internetmarke operations.")
            .Validate(o => !string.IsNullOrEmpty(o.ApiSecret),
                "DHL ApiSecret is required for Internetmarke operations.");

        // Register the Internetmarke-specific token service (singleton for caching)
        builder.Services.AddSingleton<DhlInternetmarkeTokenService>();
        builder.Services.AddHttpClient(DhlInternetmarkeTokenService.TokenHttpClientName);

        // Register the Internetmarke-specific auth handler
        builder.Services.AddTransient<DhlInternetmarkeAuthHandler>();

        builder.Services.AddHttpClient<DhlInternetmarkeClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.InternetmarkeBaseUrl);
        })
        .AddHttpMessageHandler<DhlInternetmarkeAuthHandler>()
        .RedactLoggedHeaders(["Authorization"]);

        builder.Services.AddTransient<IDhlInternetmarkeClient>(sp => sp.GetRequiredService<DhlInternetmarkeClient>());

        return builder;
    }
}
