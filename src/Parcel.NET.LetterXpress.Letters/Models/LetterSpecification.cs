namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// Print and shipping specification of a letter.
/// </summary>
public class LetterSpecification
{
    /// <summary>
    /// Gets or sets the print color. Defaults to <see cref="LetterColor.BlackWhite"/>.
    /// </summary>
    public LetterColor Color { get; set; } = LetterColor.BlackWhite;

    /// <summary>
    /// Gets or sets the print mode. Defaults to <see cref="PrintMode.Simplex"/>.
    /// </summary>
    public PrintMode Mode { get; set; } = PrintMode.Simplex;

    /// <summary>
    /// Gets or sets the shipping destination. Defaults to <see cref="ShippingType.Auto"/>.
    /// </summary>
    public ShippingType Shipping { get; set; } = ShippingType.Auto;

    /// <summary>
    /// Gets or sets a value indicating whether to use a C4 envelope.
    /// Optional, applies to letters under 9 sheets.
    /// </summary>
    public bool C4 { get; set; }
}
