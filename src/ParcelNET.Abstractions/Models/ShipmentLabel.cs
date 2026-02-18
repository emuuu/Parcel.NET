namespace ParcelNET.Abstractions.Models;

/// <summary>
/// A shipping label returned by the carrier.
/// </summary>
public class ShipmentLabel
{
    /// <summary>
    /// Gets the format of the label content (e.g. PDF, ZPL).
    /// </summary>
    public LabelFormat Format { get; init; }

    /// <summary>
    /// Gets the raw label content as a byte array.
    /// </summary>
    public required byte[] Content { get; init; }
}

/// <summary>
/// Supported label output formats.
/// </summary>
public enum LabelFormat
{
    /// <summary>PDF format.</summary>
    Pdf,
    /// <summary>ZPL (Zebra Programming Language) format.</summary>
    Zpl
}
