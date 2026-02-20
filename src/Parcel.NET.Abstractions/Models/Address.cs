namespace Parcel.NET.Abstractions.Models;

/// <summary>
/// Represents a postal address for shipping or delivery.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets the name of the person or company at this address.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the second address name line (e.g. department, c/o).
    /// </summary>
    public string? Name2 { get; init; }

    /// <summary>
    /// Gets the third address name line.
    /// </summary>
    public string? Name3 { get; init; }

    /// <summary>
    /// Gets the street name.
    /// </summary>
    public string? Street { get; init; }

    /// <summary>
    /// Gets the house or building number.
    /// </summary>
    public string? HouseNumber { get; init; }

    /// <summary>
    /// Gets the postal or ZIP code.
    /// </summary>
    public required string PostalCode { get; init; }

    /// <summary>
    /// Gets the city name.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-3 country code (e.g. "DEU").
    /// </summary>
    public required string CountryCode { get; init; }

    /// <summary>
    /// Gets the state or province, if applicable.
    /// </summary>
    public string? State { get; init; }
}
