namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// User profile data from the Internetmarke account (GET /user/profile).
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the EKP (Kundennummer).
    /// </summary>
    public string? Ekp { get; set; }

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Gets or sets the salutation.
    /// </summary>
    public string? Salutation { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? Firstname { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? Lastname { get; set; }

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public string? HouseNo { get; set; }

    /// <summary>
    /// Gets or sets the postal code / ZIP.
    /// </summary>
    public string? Zip { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the country code (e.g. "DEU").
    /// </summary>
    public string? Country { get; set; }
}
