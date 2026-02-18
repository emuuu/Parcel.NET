using Microsoft.Extensions.DependencyInjection;

namespace ParcelNET.Dhl;

/// <summary>
/// Builder for chaining DHL sub-service registrations (shipping, tracking) after calling
/// <see cref="DhlServiceCollectionExtensions.AddDhl"/>.
/// </summary>
public class DhlBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    internal DhlBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
