using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Parcel.NET.Dhl.Returns;

/// <summary>
/// Extension methods for registering the DHL Returns client.
/// </summary>
public static class DhlReturnsExtensions
{
    /// <summary>
    /// Adds the DHL Parcel DE Returns client to the service collection.
    /// Requires OAuth credentials (Username, Password, ApiSecret) to be configured.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlReturns(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.Username), "DHL Username is required for returns operations.")
            .Validate(o => !string.IsNullOrEmpty(o.Password), "DHL Password is required for returns operations.")
            .Validate(o => !string.IsNullOrEmpty(o.ApiSecret), "DHL ApiSecret is required for returns operations.");

        builder.Services.AddHttpClient<DhlReturnsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
            client.BaseAddress = new Uri(options.ReturnsBaseUrl);
        })
        .AddHttpMessageHandler<DhlAuthHandler>()
        .RedactLoggedHeaders(["Authorization"]);

        builder.Services.AddTransient<IDhlReturnsClient>(sp => sp.GetRequiredService<DhlReturnsClient>());

        return builder;
    }
}
