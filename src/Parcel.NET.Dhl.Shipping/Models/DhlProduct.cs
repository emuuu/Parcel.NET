namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// DHL shipping product codes.
/// </summary>
public enum DhlProduct
{
    /// <summary>DHL Paket (national).</summary>
    V01PAK,
    /// <summary>DHL Paket International.</summary>
    V53WPAK,
    /// <summary>DHL Europaket.</summary>
    V54EPAK,
    /// <summary>DHL Warenpost (national).</summary>
    V62WP,
    /// <summary>DHL Warenpost International.</summary>
    V66WPI
}
