using System.Text.Json;
using Parcel.NET.Dhl.Internal;
using Parcel.NET.Dhl.Shipping.Models;

namespace Parcel.NET.Dhl.Shipping.Internal;

internal static class DhlShippingErrorParser
{
    private const string UnknownValidationMessage = "Unknown validation error";

    // Reads items[].validationMessages[] from a DHL Shipping response body. Returns false if the
    // body is not a JSON object. Never throws — malformed parts of the body are skipped.
    internal static bool TryParseValidationMessages(string? rawBody, out List<ValidationMessage> messages)
    {
        messages = [];

        if (string.IsNullOrEmpty(rawBody))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (!root.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                return true;
            }

            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object ||
                    !item.TryGetProperty("validationMessages", out var validationMessages) ||
                    validationMessages.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var entry in validationMessages.EnumerateArray())
                {
                    if (entry.ValueKind == JsonValueKind.Object)
                        messages.Add(MapValidationMessage(entry));
                }
            }

            return true;
        }
        catch (JsonException)
        {
            // Body is not valid JSON
            return false;
        }
    }

    internal static List<ValidationMessage> ParseValidationMessages(string? rawBody)
    {
        TryParseValidationMessages(rawBody, out var messages);
        return messages;
    }

    // Only "Error" blocks a shipment; any other or missing state is treated as a warning.
    internal static ValidationSeverity MapSeverity(string? validationState) =>
        string.Equals(validationState, "Error", StringComparison.OrdinalIgnoreCase)
            ? ValidationSeverity.Error
            : ValidationSeverity.Warning;

    private static ValidationMessage MapValidationMessage(JsonElement entry) =>
        new()
        {
            Property = DhlErrorHelper.TryGetNonEmptyString(entry, "property", out var property) ? property : null,
            Message = DhlErrorHelper.TryGetNonEmptyString(entry, "validationMessage", out var text) ? text : UnknownValidationMessage,
            Severity = MapSeverity(DhlErrorHelper.TryGetNonEmptyString(entry, "validationState", out var state) ? state : null)
        };
}
