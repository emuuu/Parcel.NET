using Parcel.NET.Abstractions.Exceptions;

namespace Parcel.NET.Dhl.Tracking;

/// <summary>
/// Thrown when the Parcel DE Tracking API reports that no data exists for a tracking number
/// (response code <c>100</c>, "Keine Daten gefunden").
/// </summary>
/// <remarks>
/// Other API errors, for example <c>5</c> (login failed) or <c>62</c> (missing permission),
/// are reported as a plain <see cref="TrackingException"/>. Catching <see cref="TrackingException"/>
/// also catches this exception.
/// </remarks>
public class DhlTrackingNotFoundException : TrackingException
{
    /// <summary>
    /// The error code reported by the API for an unknown tracking number.
    /// </summary>
    internal const string NotFoundErrorCode = "100";

    /// <summary>
    /// Initializes a new instance of <see cref="DhlTrackingNotFoundException"/>.
    /// </summary>
    /// <param name="trackingNumber">The tracking number that was not found.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="rawResponse">The raw XML response returned by the API, if available.</param>
    public DhlTrackingNotFoundException(string trackingNumber, string message, string? rawResponse)
        : base(message, statusCode: null, errorCode: NotFoundErrorCode, rawResponse: rawResponse)
    {
        TrackingNumber = trackingNumber;
    }

    /// <summary>
    /// Gets the tracking number for which the API returned no data.
    /// </summary>
    public string TrackingNumber { get; }
}
