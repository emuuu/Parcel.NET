namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// Result of a DHL manifest creation request.
/// </summary>
public class ManifestResult
{
    /// <summary>
    /// Gets a value indicating whether the manifest was created successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets an optional message with additional details about the result.
    /// </summary>
    public string? Message { get; init; }
}
