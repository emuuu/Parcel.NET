using System.Net;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Shipping.Models;

namespace Parcel.NET.Dhl.Shipping;

/// <summary>
/// Exception thrown when the DHL Shipping API rejects a shipment (HTTP 400/422) and the
/// response contains at least one validation message with state <c>Error</c>.
/// </summary>
/// <remarks>
/// The exception message lists the affected property paths and message texts exactly as returned
/// by DHL; Parcel.NET adds no values from the request. Whether DHL quotes input values in its
/// texts is outside the library's control. The full response body, which may contain personal
/// data, is available via <see cref="ParcelException.RawResponse"/>.
/// </remarks>
public class DhlShippingValidationException : ShippingException
{
    /// <summary>
    /// Gets the validation messages with state <c>Error</c> that caused the rejection. Never empty.
    /// </summary>
    public IReadOnlyList<ValidationMessage> ValidationMessages { get; }

    /// <summary>
    /// Gets the remaining validation messages of the response, reported with <c>Warning</c> severity.
    /// </summary>
    public IReadOnlyList<ValidationMessage> Warnings { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DhlShippingValidationException"/>.
    /// </summary>
    public DhlShippingValidationException(
        string message,
        IReadOnlyList<ValidationMessage> validationMessages,
        IReadOnlyList<ValidationMessage> warnings,
        HttpStatusCode? statusCode,
        string? errorCode,
        string? rawResponse,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, rawResponse, innerException)
    {
        ArgumentNullException.ThrowIfNull(validationMessages);
        ArgumentNullException.ThrowIfNull(warnings);
        ValidationMessages = validationMessages;
        Warnings = warnings;
    }
}
