using System.Text.Json;

namespace Parcel.NET.Dhl.Internal;

internal static class DhlErrorHelper
{
    internal static string TryParseErrorDetail(string? rawBody)
    {
        if (string.IsNullOrEmpty(rawBody))
            return rawBody ?? string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("status", out var status) &&
                status.ValueKind == JsonValueKind.Object &&
                status.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? rawBody;
            }
            if (doc.RootElement.TryGetProperty("detail", out var topDetail))
            {
                return topDetail.GetString() ?? rawBody;
            }
        }
        catch (JsonException)
        {
            // Fall through — raw body is not valid JSON
        }

        return rawBody;
    }
}
