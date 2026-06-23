using System.Net;
using Parcel.NET.Abstractions.Exceptions;

namespace Parcel.NET.LetterXpress;

/// <summary>
/// Exception thrown when a LetterXpress API operation fails.
/// </summary>
public class LetterXpressException : ParcelException
{
    /// <summary>
    /// Initializes a new instance of <see cref="LetterXpressException"/>.
    /// </summary>
    public LetterXpressException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="LetterXpressException"/> with an inner exception.
    /// </summary>
    public LetterXpressException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="LetterXpressException"/> with carrier API error details.
    /// </summary>
    public LetterXpressException(
        string message,
        HttpStatusCode? statusCode,
        string? errorCode,
        string? rawResponse,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, rawResponse, innerException) { }
}
