namespace ParcelNET.Dhl.Shipping.Models;

/// <summary>
/// Result of a DHL shipment validation request.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the shipment data is valid.
    /// </summary>
    public bool Valid { get; init; }

    /// <summary>
    /// Gets the list of validation messages. May contain warnings even when <see cref="Valid"/> is true.
    /// </summary>
    public List<ValidationMessage> Messages { get; init; } = [];
}

/// <summary>
/// A single validation message from the DHL API.
/// </summary>
public class ValidationMessage
{
    /// <summary>
    /// Gets the property path that caused the validation issue, if available.
    /// </summary>
    public string? Property { get; init; }

    /// <summary>
    /// Gets the validation message text.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the severity level of this validation issue.
    /// </summary>
    public ValidationSeverity Severity { get; init; }
}

/// <summary>
/// Severity level of a DHL validation message.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>Non-critical issue that does not prevent shipment creation.</summary>
    Warning,
    /// <summary>Critical issue that prevents shipment creation.</summary>
    Error
}
