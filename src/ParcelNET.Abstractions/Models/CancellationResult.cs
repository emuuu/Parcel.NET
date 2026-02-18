namespace ParcelNET.Abstractions.Models;

/// <summary>
/// Result of a shipment cancellation request.
/// </summary>
public class CancellationResult
{
    /// <summary>
    /// Gets a value indicating whether the cancellation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets an optional message with additional details about the result.
    /// </summary>
    public string? Message { get; init; }
}
