using Parcel.NET.Docs.Playground.Models;

namespace Parcel.NET.Docs.Playground.Services;

/// <summary>
/// Scoped service holding user credentials in-memory per SignalR circuit.
/// When the tab closes, the circuit ends and credentials are garbage collected.
/// </summary>
public class PlaygroundSessionService
{
    public DhlCredentials DhlCredentials { get; } = new();
    public GoExpressCredentials GoExpressCredentials { get; } = new();
}
