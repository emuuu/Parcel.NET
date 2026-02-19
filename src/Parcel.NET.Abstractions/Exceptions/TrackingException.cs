using System.Net;

namespace Parcel.NET.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a tracking API operation fails.
/// </summary>
public class TrackingException : ParcelNetException
{
    /// <summary>
    /// Initializes a new instance of <see cref="TrackingException"/>.
    /// </summary>
    public TrackingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="TrackingException"/> with an inner exception.
    /// </summary>
    public TrackingException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="TrackingException"/> with carrier API error details.
    /// </summary>
    public TrackingException(
        string message,
        HttpStatusCode? statusCode,
        string? errorCode,
        string? rawResponse,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, rawResponse, innerException) { }
}
