namespace Parcel.NET.GoExpress.Shipping.Models;

/// <summary>
/// Indicates whether a delivery/pickup is on a weekend or holiday.
/// </summary>
public enum WeekendOrHolidayIndicator
{
    /// <summary>No special indicator — normal business day.</summary>
    None,
    /// <summary>Saturday delivery/pickup.</summary>
    Saturday,
    /// <summary>Holiday delivery/pickup.</summary>
    Holiday
}
