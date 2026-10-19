using System.Net;
using System.Text.Json;

namespace Parcel.NET.Dhl.Internal;

internal static class DhlErrorHelper
{
    // Builds the exception message for a failed DHL API call. The raw body may contain
    // addresses and other personal data, so it is never part of the message — callers keep
    // it in ParcelException.RawResponse instead.
    internal static string FormatErrorMessage(string apiName, HttpStatusCode statusCode, string? rawBody)
    {
        var detail = TryGetErrorDetail(rawBody);
        return detail is not null
            ? $"{apiName} returned {(int)statusCode}: {detail}"
            : $"{apiName} returned {(int)statusCode} without error details.";
    }

    // Returns status.detail or the top-level detail of a DHL error body, or null if neither is
    // present. Never throws.
    internal static string? TryGetErrorDetail(string? rawBody)
    {
        if (string.IsNullOrEmpty(rawBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            if (root.TryGetProperty("status", out var status) &&
                status.ValueKind == JsonValueKind.Object &&
                TryGetNonEmptyString(status, "detail", out var detail))
            {
                return detail;
            }
            if (TryGetNonEmptyString(root, "detail", out var topDetail))
            {
                return topDetail;
            }
        }
        catch (JsonException)
        {
            // Fall through — raw body is not valid JSON
        }

        return null;
    }

    // Kept for binary compatibility: DHL packages released before 1.1.0 call this method via
    // InternalsVisibleTo. It no longer returns the raw body.
    [Obsolete("Use FormatErrorMessage instead.")]
    internal static string TryParseErrorDetail(string? rawBody)
        => TryGetErrorDetail(rawBody) ?? "DHL API request failed.";

    internal static bool TryGetNonEmptyString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;

        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        try
        {
            if (property.GetString() is { Length: > 0 } text)
            {
                value = text;
                return true;
            }
        }
        catch (InvalidOperationException)
        {
            // String token cannot be decoded (e.g. unpaired surrogate escape like "\uD800")
        }

        return false;
    }
}
