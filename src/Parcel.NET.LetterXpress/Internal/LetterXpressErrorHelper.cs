using System.Text.Json;

namespace Parcel.NET.LetterXpress.Internal;

internal static class LetterXpressErrorHelper
{
    /// <summary>
    /// Extracts a readable error detail from a LetterXpress error body. Never throws; falls back to the raw body.
    /// </summary>
    /// <remarks>
    /// The <c>message</c> (or <c>error</c>) field is either a string, or, for validation errors, an object mapping
    /// field names to arrays of messages, e.g.
    /// <c>{"status":400,"message":{"letter.registered":["The letter.registered field is prohibited ..."]}}</c>,
    /// which is flattened to <c>letter.registered: The letter.registered field is prohibited ...</c>.
    /// </remarks>
    internal static string TryParseErrorDetail(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return rawBody;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return rawBody;
            }

            if (doc.RootElement.TryGetProperty("message", out var message)
                && FormatDetail(message) is { Length: > 0 } messageDetail)
            {
                return messageDetail;
            }

            if (doc.RootElement.TryGetProperty("error", out var error)
                && FormatDetail(error) is { Length: > 0 } errorDetail)
            {
                return errorDetail;
            }
        }
        catch (Exception)
        {
            // Fall through - the body is not valid JSON or has an unexpected shape.
        }

        return rawBody;
    }

    private static string? FormatDetail(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Object => JoinNonEmpty(element.EnumerateObject().Select(FormatField)),
        JsonValueKind.Array => JoinNonEmpty(element.EnumerateArray().Select(FormatDetail)),
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
        _ => null
    };

    private static string? FormatField(JsonProperty property)
    {
        var detail = FormatDetail(property.Value);
        return string.IsNullOrEmpty(detail) ? null : $"{property.Name}: {detail}";
    }

    private static string? JoinNonEmpty(IEnumerable<string?> parts)
    {
        var joined = string.Join("; ", parts.Where(p => !string.IsNullOrEmpty(p)));
        return joined.Length > 0 ? joined : null;
    }
}
