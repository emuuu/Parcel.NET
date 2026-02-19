namespace Parcel.NET.GoExpress.Shipping.Models;

/// <summary>
/// GO! Express service types.
/// </summary>
public enum GoExpressService
{
    /// <summary>General Overnight - national overnight delivery.</summary>
    ON,
    /// <summary>International delivery.</summary>
    INT,
    /// <summary>Letter Express - national document delivery.</summary>
    LET,
    /// <summary>International Letter Express.</summary>
    INL,
    /// <summary>Parcel Service National.</summary>
    PSN,
    /// <summary>Parcel Service International.</summary>
    PSI,
    /// <summary>Overnight City - same-city overnight.</summary>
    ONC,
    /// <summary>Letter Express City - same-city letter.</summary>
    LEC,
    /// <summary>Direct / Individual - custom direct transport.</summary>
    DI
}
