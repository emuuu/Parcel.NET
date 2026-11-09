using System.Globalization;
using Parcel.NET.Dhl.Tracking.Internal;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Tracking.Tests;

public class DhlTrackingTimestampTests
{
    private static readonly TimeSpan Winter = TimeSpan.FromHours(1);
    private static readonly TimeSpan Summer = TimeSpan.FromHours(2);

    [Theory]
    [InlineData("15.01.2026 12:00", 2026, 1, 15, 12, 0, 1)]   // CET
    [InlineData("15.07.2026 12:00", 2026, 7, 15, 12, 0, 2)]   // CEST
    [InlineData("28.03.2026 23:59", 2026, 3, 28, 23, 59, 1)]  // day before the switch to summer time
    [InlineData("29.03.2026 03:00", 2026, 3, 29, 3, 0, 2)]    // first hour of summer time
    [InlineData("25.10.2026 01:59", 2026, 10, 25, 1, 59, 2)]  // before the overlap
    [InlineData("25.10.2026 03:00", 2026, 10, 25, 3, 0, 1)]   // after the overlap
    public void ParseTimestamp_GermanFormat_UsesBerlinOffset(string value, int year, int month, int day, int hour, int minute, int offsetHours)
    {
        var result = DhlTrackingTimestamp.ParseTimestamp(value);

        result.ShouldBe(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(offsetHours)));
        result!.Value.Offset.ShouldBe(TimeSpan.FromHours(offsetHours));
    }

    [Fact]
    public void ParseTimestamp_NonExistentLocalTime_DoesNotThrow()
    {
        // 29 March 2026 02:30 does not exist in Berlin (clocks jump from 02:00 to 03:00).
        var result = DhlTrackingTimestamp.ParseTimestamp("29.03.2026 02:30");

        result.ShouldNotBeNull();
        result!.Value.Offset.ShouldBe(Winter);
    }

    [Fact]
    public void ParseTimestamp_AmbiguousLocalTime_UsesStandardTime()
    {
        // 25 October 2026 02:30 occurs twice in Berlin; the winter time (+01:00) is used.
        var result = DhlTrackingTimestamp.ParseTimestamp("25.10.2026 02:30");

        result.ShouldNotBeNull();
        result!.Value.Offset.ShouldBe(Winter);
    }

    [Fact]
    public void ParseTimestamp_IsIndependentOfCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            foreach (var culture in new[] { "en-US", "en-GB", "fr-FR", "ja-JP", "" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);

                // Day <= 12: a month-first culture would swap day and month.
                DhlTrackingTimestamp.ParseTimestamp("03.04.2026 10:00")
                    .ShouldBe(new DateTimeOffset(2026, 4, 3, 10, 0, 0, Summer), culture);
                DhlTrackingTimestamp.ParseDeliveryDate("03.04.2026")
                    .ShouldBe(new DateTimeOffset(2026, 4, 3, 0, 0, 0, Summer), culture);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("2026-02-18T10:00:00+01:00", 10, 1)]
    [InlineData("2026-02-18T10:00:00Z", 10, 0)]
    [InlineData("2026-02-18T10:00:00", 10, 0)]         // no offset: UTC, independent of the host time zone
    [InlineData("2026-02-18T10:00:00.123", 10, 0)]
    [InlineData("2026-02-18T10:00", 10, 0)]
    public void ParseTimestamp_Iso8601Fallback(string value, int hour, int offsetHours)
    {
        var result = DhlTrackingTimestamp.ParseTimestamp(value);

        result.ShouldNotBeNull();
        result!.Value.Hour.ShouldBe(hour);
        result.Value.Offset.ShouldBe(TimeSpan.FromHours(offsetHours));
        result.Value.Date.ShouldBe(new DateTime(2026, 2, 18));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("not a date")]
    [InlineData("18.02.2026")]        // date without time is not a valid event timestamp
    [InlineData("02/18/2026 10:00")]  // culture-specific formats are not accepted
    [InlineData("31.02.2026 10:00")]
    [InlineData("01.01.0001 00:00")]  // not representable with an offset
    public void ParseTimestamp_Invalid_ReturnsNull(string? value)
        => DhlTrackingTimestamp.ParseTimestamp(value).ShouldBeNull();

    [Theory]
    [InlineData("20.02.2026", 2026, 2, 20, 0, 0, 1)]
    [InlineData("2026-02-20", 2026, 2, 20, 0, 0, 1)]
    [InlineData("20.07.2026", 2026, 7, 20, 0, 0, 2)]
    [InlineData("20.02.2026 14:30", 2026, 2, 20, 14, 30, 1)]
    public void ParseDeliveryDate_Formats(string value, int year, int month, int day, int hour, int minute, int offsetHours)
        => DhlTrackingTimestamp.ParseDeliveryDate(value)
            .ShouldBe(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(offsetHours)));

    [Fact]
    public void ParseDeliveryDate_Iso8601WithTime_FallsBack()
        => DhlTrackingTimestamp.ParseDeliveryDate("2026-02-20T08:00:00+01:00")
            .ShouldBe(new DateTimeOffset(2026, 2, 20, 8, 0, 0, Winter));

    [Fact]
    public void FallbackTimeZone_MatchesBerlinAroundTheYear()
    {
        var fallback = DhlTrackingTimestamp.CreateCentralEuropeanTimeZone();

        var samples = new List<DateTime>();
        for (var year = 2024; year <= 2030; year++)
        {
            for (var month = 1; month <= 12; month++)
            {
                samples.Add(new DateTime(year, month, 1, 12, 0, 0, DateTimeKind.Unspecified));
                samples.Add(new DateTime(year, month, 15, 12, 0, 0, DateTimeKind.Unspecified));
                samples.Add(new DateTime(year, month, 28, 12, 0, 0, DateTimeKind.Unspecified));
            }

            // Days around both transitions (last Sunday of March / October).
            foreach (var month in new[] { 3, 10 })
            {
                var lastSunday = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                while (lastSunday.DayOfWeek != DayOfWeek.Sunday) lastSunday = lastSunday.AddDays(-1);
                samples.Add(lastSunday.AddDays(-1).AddHours(12));
                samples.Add(lastSunday.AddHours(1));
                samples.Add(lastSunday.AddHours(4));
                samples.Add(lastSunday.AddDays(1).AddHours(12));
            }
        }

        foreach (var local in samples)
        {
            fallback.GetUtcOffset(local).ShouldBe(DhlTrackingTimestamp.GetBerlinOffset(local), local.ToString("O"));
        }
    }
}
