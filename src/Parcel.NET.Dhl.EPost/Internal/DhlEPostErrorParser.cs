using System.Text.Json;

namespace Parcel.NET.Dhl.EPost.Internal;

/// <summary>Parses the E-POST <c>Error</c> object ({level, code, description}) returned on error responses.</summary>
internal static class DhlEPostErrorParser
{
    /// <summary>
    /// Attempts to parse an <see cref="DhlEPostError"/> from a response body.
    /// Returns <c>null</c> for an empty, non-JSON, or code-less body.
    /// </summary>
    public static DhlEPostError? TryParse(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        try
        {
            var error = JsonSerializer.Deserialize(rawBody, DhlEPostJsonContext.Default.DhlEPostError);
            return string.IsNullOrEmpty(error?.Code) ? null : error;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
