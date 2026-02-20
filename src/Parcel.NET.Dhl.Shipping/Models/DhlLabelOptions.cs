using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// DHL-specific label generation options.
/// </summary>
public class DhlLabelOptions
{
    /// <summary>
    /// Gets the desired label output format. Defaults to <see cref="LabelFormat.Pdf"/>.
    /// </summary>
    public LabelFormat Format { get; init; } = LabelFormat.Pdf;

    /// <summary>
    /// Gets the paper size for the label. Defaults to <see cref="DhlPrintFormat.A4"/>.
    /// </summary>
    public DhlPrintFormat PrintFormat { get; init; } = DhlPrintFormat.A4;

    /// <summary>
    /// Gets a value indicating whether to combine multiple labels into a single document.
    /// </summary>
    public bool Combine { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include additional documents (e.g. customs forms).
    /// </summary>
    public bool IncludeDocs { get; init; }
}

/// <summary>
/// DHL label paper size / print formats as defined by the DHL Parcel DE Shipping API v2.
/// </summary>
public enum DhlPrintFormat
{
    /// <summary>A4 paper (210 x 297 mm).</summary>
    A4,
    /// <summary>Label 105 x 208 mm (910-300-700).</summary>
    Label_105x208,
    /// <summary>Label 105 x 208 mm without border (910-300-700-oZ).</summary>
    Label_105x208_oZ,
    /// <summary>Label 105 x 148 mm (910-300-300).</summary>
    Label_105x148,
    /// <summary>Label 105 x 148 mm without border (910-300-300-oz).</summary>
    Label_105x148_oZ,
    /// <summary>Label 105 x 209 mm (910-300-710).</summary>
    Label_105x209,
    /// <summary>Thermal label 103 x 199 mm (910-300-600).</summary>
    Thermal_103x199,
    /// <summary>Thermal label 103 x 199 mm variant (910-300-610).</summary>
    Thermal_103x199_V2,
    /// <summary>Thermal label 103 x 150 mm (910-300-400).</summary>
    Thermal_103x150,
    /// <summary>Thermal label 103 x 150 mm variant (910-300-410).</summary>
    Thermal_103x150_V2,
    /// <summary>Warenpost label 100 x 70 mm.</summary>
    Label_100x70
}
