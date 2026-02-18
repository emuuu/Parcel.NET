namespace ParcelNET.Abstractions.Models;

/// <summary>
/// Physical dimensions of a package.
/// </summary>
public class Dimensions
{
    /// <summary>
    /// Gets the length of the package.
    /// </summary>
    public double Length { get; init; }

    /// <summary>
    /// Gets the width of the package.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the height of the package.
    /// </summary>
    public double Height { get; init; }
}
