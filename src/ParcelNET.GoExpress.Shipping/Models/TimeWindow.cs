namespace ParcelNET.GoExpress.Shipping.Models;

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
    /// Gets or sets whether this is a weekend pickup/delivery.
    /// </summary>
    public bool IsWeekend { get; init; }

    /// <summary>
    /// Gets or sets whether this is a holiday pickup/delivery.
    /// </summary>
    public bool IsHoliday { get; init; }
}
