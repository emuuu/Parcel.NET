using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParcelNET.Abstractions;

namespace ParcelNET.Dhl.Shipping;

/// <summary>
/// Extension methods for registering the DHL shipping client.
/// </summary>
public static class DhlShippingExtensions
{
    /// <summary>
    /// Adds the DHL shipping client to the service collection, registered as both
    /// <see cref="IShipmentService"/> and <see cref="IDhlShippingClient"/>.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlShipping(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<DhlShippingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.ShippingBaseUrl);
        })
        .AddHttpMessageHandler<DhlAuthHandler>();

        builder.Services.AddTransient<IShipmentService>(sp => sp.GetRequiredService<DhlShippingClient>());
        builder.Services.AddTransient<IDhlShippingClient>(sp => sp.GetRequiredService<DhlShippingClient>());

        return builder;
    }
}
