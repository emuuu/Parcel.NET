using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Parcel.NET.LetterXpress.Letters;

/// <summary>
/// Extension methods for registering the LetterXpress letters client.
/// </summary>
public static class LetterXpressLettersExtensions
{
    /// <summary>
    /// Adds the LetterXpress letters client to the service collection, registered as
    /// <see cref="ILetterXpressClient"/>.
    /// </summary>
    /// <param name="builder">The LetterXpress builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static LetterXpressBuilder AddLetterXpressLetters(this LetterXpressBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpClient<LetterXpressClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<LetterXpressOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        builder.Services.AddTransient<ILetterXpressClient>(sp => sp.GetRequiredService<LetterXpressClient>());

        return builder;
    }
}
