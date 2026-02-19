namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Contact information for a shipper or consignee.
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Gets the contact person's name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? Phone { get; init; }
}
