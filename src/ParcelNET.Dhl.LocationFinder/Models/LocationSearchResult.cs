namespace ParcelNET.Dhl.LocationFinder.Models;

/// <summary>
/// Result of a DHL location search.
/// </summary>
public class LocationSearchResult
{
    /// <summary>
    /// Gets or sets the list of matching locations.
    /// </summary>
    public List<DhlLocation> Locations { get; set; } = [];
}
