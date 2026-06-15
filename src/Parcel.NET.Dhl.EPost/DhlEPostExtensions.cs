using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parcel.NET.Dhl.EPost.Internal;

namespace Parcel.NET.Dhl.EPost;

/// <summary>Extension methods for registering the Deutsche Post E-POSTBUSINESS API client.</summary>
public static class DhlEPostExtensions
{
    /// <summary>
    /// Adds the Deutsche Post E-POST hybrid-mail client to the service collection.
    /// Uses an E-POST-specific JWT auth handler (<c>POST /api/Login</c> with vendorID/EKP/secret/password),
    /// separate from the shared DHL parcel ROPC token endpoint.
    /// Requires <see cref="DhlOptions.EPostVendorId"/>, <see cref="DhlOptions.EPostEkp"/>,
    /// <see cref="DhlOptions.EPostSecret"/> and <see cref="DhlOptions.EPostPassword"/>.
    /// </summary>
    /// <param name="builder">The DHL builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static DhlBuilder AddDhlEPost(this DhlBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptionsWithValidateOnStart<DhlOptions>()
            .Validate(o => !string.IsNullOrEmpty(o.EPostVendorId), "DHL EPostVendorId is required for E-POST operations.")
            .Validate(o => !string.IsNullOrEmpty(o.EPostEkp), "DHL EPostEkp is required for E-POST operations.")
            .Validate(o => !string.IsNullOrEmpty(o.EPostSecret), "DHL EPostSecret is required for E-POST operations.")
            .Validate(o => !string.IsNullOrEmpty(o.EPostPassword), "DHL EPostPassword is required for E-POST operations.")
            .Validate(
                o => o.EPostTokenDurationMinutes is null or (>= 1 and <= 1440),
                "DHL EPostTokenDurationMinutes must be between 1 and 1440 (24h).");

        // E-POST-specific token service (singleton caching) + its own HTTP client.
        builder.Services.AddSingleton<DhlEPostTokenService>();
        builder.Services.AddHttpClient(DhlEPostTokenService.TokenHttpClientName);

        builder.Services.AddTransient<DhlEPostAuthHandler>();

        builder.Services.AddHttpClient<DhlEPostClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<DhlOptions>>().Value;
                client.BaseAddress = new Uri(options.EPostBaseUrl);
            })
            .AddHttpMessageHandler<DhlEPostAuthHandler>()
            .RedactLoggedHeaders(["Authorization"]);

        builder.Services.AddTransient<IDhlEPostClient>(sp => sp.GetRequiredService<DhlEPostClient>());

        return builder;
    }
}
