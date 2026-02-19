using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl.Internetmarke;

/// <summary>
/// Extension methods for registering the DHL Internetmarke client.
/// </summary>
public static class DhlInternetmarkeExtensions
{
    /// <summary>
    /// Adds the DHL Post DE Internetmarke client to the service collection.
    /// Requires OAuth credentials. Uses <see cref="DhlOptions.InternetmarkeUsername"/>/<see cref="DhlOptions.InternetmarkePassword"/>
    /// if set, otherwise falls back to <see cref="DhlOptions.Username"/>/<see cref="DhlOptions.Password"/>.
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

        builder.Services.AddHttpClient<DhlInternetmarkeClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.InternetmarkeBaseUrl);
        })
        .AddHttpMessageHandler<DhlAuthHandler>()
        .RedactLoggedHeaders(["Authorization"]);

        builder.Services.AddTransient<IDhlInternetmarkeClient>(sp => sp.GetRequiredService<DhlInternetmarkeClient>());

        return builder;
    }
}
