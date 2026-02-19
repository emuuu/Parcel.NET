using System.Text.Json;

namespace ParcelNET.GoExpress.Internal;

internal static class GoExpressErrorHelper
{
    internal static string TryParseErrorDetail(string rawBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? rawBody;
            }
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString() ?? rawBody;
            }
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? rawBody;
            }
        }
        catch (JsonException)
        {
            // Fall through - raw body is not valid JSON
        }

        return rawBody;
    }
}
