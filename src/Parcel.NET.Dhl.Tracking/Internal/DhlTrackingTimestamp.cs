using System.Globalization;

namespace Parcel.NET.Dhl.Tracking.Internal;

/// <summary>
/// Parses the timestamps of Parcel DE Tracking v0 responses.
/// </summary>
/// <remarks>
/// DHL returns timestamps as <c>dd.MM.yyyy HH:mm</c> without a time zone. The time zone is not documented by DHL;
/// the values are assumed to be German local time (Europe/Berlin, including daylight saving time).
/// The exact format is tried first with the invariant culture, so the result never depends on the culture of the
/// current process. Values that do not match fall back to ISO 8601.
/// </remarks>
internal static class DhlTrackingTimestamp
{
    private static readonly string[] GermanDateTimeFormats = ["dd.MM.yyyy HH:mm"];
    private static readonly string[] GermanDateFormats = ["dd.MM.yyyy", "yyyy-MM-dd"];

    private static readonly string[] IsoFormats =
    [
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd'T'HH:mmK",
        "yyyy-MM-dd"
    ];

    private static readonly TimeZoneInfo BerlinTimeZone = ResolveBerlinTimeZone();

    /// <summary>
    /// Parses an event or status timestamp (<c>dd.MM.yyyy HH:mm</c>, then ISO 8601).
    /// </summary>
    internal static DateTimeOffset? ParseTimestamp(string? value)
        => Parse(value, GermanDateTimeFormats);

    /// <summary>
    /// Parses a delivery date (<c>dd.MM.yyyy HH:mm</c>, <c>dd.MM.yyyy</c>, <c>yyyy-MM-dd</c>, then ISO 8601).
    /// A date without a time of day is interpreted as midnight local time.
    /// </summary>
    internal static DateTimeOffset? ParseDeliveryDate(string? value)
        => Parse(value, [.. GermanDateTimeFormats, .. GermanDateFormats]);

    /// <summary>
    /// Gets the UTC offset of the local German time <paramref name="localTime"/>.
    /// A non-existent time (spring-forward gap) uses the offset reported by the time zone,
    /// an ambiguous time (autumn overlap) is resolved to standard time (+01:00).
    /// </summary>
    internal static TimeSpan GetBerlinOffset(DateTime localTime)
        => GetOffset(BerlinTimeZone, DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified));

    private static DateTimeOffset? Parse(string? value, string[] germanFormats)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();

        if (DateTime.TryParseExact(text, germanFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local))
        {
            try
            {
                return new DateTimeOffset(local, GetBerlinOffset(local));
            }
            catch (ArgumentOutOfRangeException)
            {
                // Values at the very edge of the DateTime range cannot be represented with an offset.
                return null;
            }
        }

        // ISO 8601 fallback. A value without an offset is treated as UTC (AssumeUniversal) so that the result
        // is deterministic and independent of the time zone of the host; DHL itself documents no ISO timestamps.
        if (DateTimeOffset.TryParseExact(text, IsoFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var iso))
            return iso;

        return null;
    }

    private static TimeSpan GetOffset(TimeZoneInfo zone, DateTime unspecified)
    {
        if (zone.IsInvalidTime(unspecified))
            return zone.GetUtcOffset(unspecified);

        if (zone.IsAmbiguousTime(unspecified))
            return zone.BaseUtcOffset;

        return zone.GetUtcOffset(unspecified);
    }

    private static TimeZoneInfo ResolveBerlinTimeZone()
    {
        foreach (var id in new[] { "Europe/Berlin", "W. Europe Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return CreateCentralEuropeanTimeZone();
    }

    /// <summary>
    /// Last resort for hosts without time zone data (for example minimal container images):
    /// UTC+1 with the EU daylight saving rule (last Sunday of March 02:00 to last Sunday of October 03:00).
    /// </summary>
    internal static TimeZoneInfo CreateCentralEuropeanTimeZone()
    {
        var start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday);
        var end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday);
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1), start, end);

        return TimeZoneInfo.CreateCustomTimeZone(
            "Central European Time (fallback)", TimeSpan.FromHours(1), "Central European Time", "CET", "CEST", [rule]);
    }
}
