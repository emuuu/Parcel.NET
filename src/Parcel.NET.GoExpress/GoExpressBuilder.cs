using Microsoft.Extensions.DependencyInjection;

namespace Parcel.NET.GoExpress;

/// <summary>
/// Builder for chaining GO! Express sub-service registrations (shipping, tracking) after calling
/// <see cref="GoExpressServiceCollectionExtensions.AddGoExpress"/>.
/// </summary>
public class GoExpressBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    internal GoExpressBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
