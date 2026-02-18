using System.Net;

namespace ParcelNET.Abstractions.Exceptions;

/// <summary>
/// Base exception for all ParcelNET operations.
/// </summary>
public class ParcelNetException : Exception
{
    /// <summary>
    /// Gets the HTTP status code returned by the carrier API, if available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the carrier-specific error code, if available.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the raw response body from the carrier API, if available.
    /// </summary>
    public string? RawResponse { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ParcelNetException"/>.
    /// </summary>
    public ParcelNetException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ParcelNetException"/> with an inner exception.
    /// </summary>
    public ParcelNetException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ParcelNetException"/> with carrier API error details.
    /// </summary>
    public ParcelNetException(
        string message,
        HttpStatusCode? statusCode,
        string? errorCode,
        string? rawResponse,
        Exception? innerException = null)
        : base(message, innerException!)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RawResponse = rawResponse;
    }
}
