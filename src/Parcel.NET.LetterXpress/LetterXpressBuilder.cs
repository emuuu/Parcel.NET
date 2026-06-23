using Microsoft.Extensions.DependencyInjection;

namespace Parcel.NET.LetterXpress;

/// <summary>
/// Builder for chaining LetterXpress sub-service registrations after calling
/// <see cref="LetterXpressServiceCollectionExtensions.AddLetterXpress"/>.
/// </summary>
public class LetterXpressBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    internal LetterXpressBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
