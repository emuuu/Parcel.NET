namespace Parcel.NET.GoExpress.Shipping.Models;

/// <summary>
/// Represents a pickup or delivery time window for GO! Express shipments.
/// </summary>
public class TimeWindow
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Gets or sets the earliest time.
    /// </summary>
    public TimeOnly? TimeFrom { get; init; }

    /// <summary>
    /// Gets or sets the latest time.
    /// </summary>
    public TimeOnly? TimeTill { get; init; }

    /// <summary>
    /// Gets or sets the earliest avis (notification) time.
    /// </summary>
    public TimeOnly? AvisFrom { get; init; }

    /// <summary>
    /// Gets or sets the latest avis (notification) time.
    /// </summary>
    public TimeOnly? AvisTill { get; init; }

    /// <summary>
    /// Gets or sets the weekend or holiday indicator for this time window.
    /// </summary>
    public WeekendOrHolidayIndicator WeekendOrHolidayIndicator { get; init; }
}
