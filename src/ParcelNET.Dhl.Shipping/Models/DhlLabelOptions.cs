using ParcelNET.Abstractions.Models;

namespace ParcelNET.Dhl.Shipping.Models;

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
/// DHL label paper size formats.
/// </summary>
public enum DhlPrintFormat
{
    /// <summary>A4 paper (210 x 297 mm).</summary>
    A4,
    /// <summary>A6 label (105 x 148 mm).</summary>
    A6,
    /// <summary>A7 label (74 x 105 mm).</summary>
    A7
}
