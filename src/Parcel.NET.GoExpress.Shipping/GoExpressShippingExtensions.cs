using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions;

namespace Parcel.NET.GoExpress.Shipping;

/// <summary>
/// Extension methods for registering the GO! Express shipping client.
/// </summary>
public static class GoExpressShippingExtensions
{
    /// <summary>
    /// Adds the GO! Express shipping client to the service collection, registered as both
    /// <see cref="IShipmentService"/> and <see cref="IGoExpressShippingClient"/>.
    /// </summary>
    /// <param name="builder">The GO! Express builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static GoExpressBuilder AddGoExpressShipping(this GoExpressBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<GoExpressOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.ResponsibleStation), "GO! Express ResponsibleStation is required for shipping operations.")
            .Validate(o => o.ResponsibleStation?.Length == 3, "GO! Express ResponsibleStation must be exactly 3 characters.");

        builder.Services.AddHttpClient<GoExpressShippingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GoExpressOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddHttpMessageHandler<GoExpressBasicAuthHandler>()
        .RedactLoggedHeaders(["Authorization"]);

        builder.Services.AddTransient<IShipmentService>(sp => sp.GetRequiredService<GoExpressShippingClient>());
        builder.Services.AddTransient<IGoExpressShippingClient>(sp => sp.GetRequiredService<GoExpressShippingClient>());

        return builder;
    }
}
