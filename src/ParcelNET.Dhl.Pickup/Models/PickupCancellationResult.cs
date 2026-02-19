namespace ParcelNET.Dhl.Pickup.Models;

/// <summary>
/// Result of a pickup order cancellation request.
/// </summary>
public class PickupCancellationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the cancellation was successful.
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// Gets or sets a human-readable status message.
    /// </summary>
    public string? Message { get; set; }
}
