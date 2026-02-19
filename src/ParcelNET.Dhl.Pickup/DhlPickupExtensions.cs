using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ParcelNET.Dhl.Pickup;

/// <summary>
/// Extension methods for registering the DHL pickup client.
/// </summary>
public static class DhlPickupExtensions
{
    /// <summary>
    /// Adds the DHL pickup client to the service collection, registered as
    /// <see cref="IDhlPickupClient"/>.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlPickup(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.Username), "DHL Username is required for pickup operations.")
            .Validate(o => !string.IsNullOrEmpty(o.Password), "DHL Password is required for pickup operations.")
            .Validate(o => !string.IsNullOrEmpty(o.ApiSecret), "DHL ApiSecret is required for pickup operations.");

        builder.Services.AddHttpClient<DhlPickupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.PickupBaseUrl);
        })
        .AddHttpMessageHandler<DhlAuthHandler>()
        .RedactLoggedHeaders(["dhl-api-key", "Authorization"]);

        builder.Services.AddTransient<IDhlPickupClient>(sp => sp.GetRequiredService<DhlPickupClient>());

        return builder;
    }
}
