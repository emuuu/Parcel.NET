namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Represents a package to be shipped.
/// </summary>
public class Package
{
    /// <summary>
    /// Gets the weight of the package.
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Gets the unit of measurement for <see cref="Weight"/>. Defaults to <see cref="WeightUnit.Kilogram"/>.
    /// </summary>
    public WeightUnit WeightUnit { get; init; } = WeightUnit.Kilogram;

    /// <summary>
    /// Gets the physical dimensions of the package, if specified.
    /// </summary>
    public Dimensions? Dimensions { get; init; }

    /// <summary>
    /// Gets the unit of measurement for <see cref="Dimensions"/>. Defaults to <see cref="DimensionUnit.Centimeter"/>.
    /// </summary>
    public DimensionUnit DimensionUnit { get; init; } = DimensionUnit.Centimeter;
}

/// <summary>
/// Unit of measurement for package weight.
/// </summary>
public enum WeightUnit
{
    /// <summary>Kilogram (kg).</summary>
    Kilogram,
    /// <summary>Gram (g).</summary>
    Gram,
    /// <summary>Pound (lb).</summary>
    Pound,
    /// <summary>Ounce (oz).</summary>
    Ounce
}

/// <summary>
/// Unit of measurement for package dimensions.
/// </summary>
public enum DimensionUnit
{
    /// <summary>Centimeter (cm).</summary>
    Centimeter,
    /// <summary>Millimeter (mm).</summary>
    Millimeter,
    /// <summary>Inch (in).</summary>
    Inch
}
