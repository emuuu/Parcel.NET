using System.Net;

namespace ParcelNET.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a shipping API operation fails.
/// </summary>
public class ShippingException : ParcelNetException
{
    /// <summary>
    /// Initializes a new instance of <see cref="ShippingException"/>.
    /// </summary>
    public ShippingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ShippingException"/> with an inner exception.
    /// </summary>
    public ShippingException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ShippingException"/> with carrier API error details.
    /// </summary>
    public ShippingException(
        string message,
        HttpStatusCode? statusCode,
        string? errorCode,
        string? rawResponse,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, rawResponse, innerException) { }
}
