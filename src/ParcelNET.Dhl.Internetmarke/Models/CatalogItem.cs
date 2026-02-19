namespace ParcelNET.Dhl.Internetmarke.Models;

/// <summary>
/// Represents a product in the Internetmarke catalog.
/// </summary>
public class CatalogItem
{
    /// <summary>
    /// Gets or sets the product ID (PPL-ID).
    /// </summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the price in EUR cents.
    /// </summary>
    public int PriceCents { get; set; }

    /// <summary>
    /// Gets or sets the product type/category.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the annotation/description.
    /// </summary>
    public string? Annotation { get; set; }

    /// <summary>
    /// Gets or sets the weight limit in grams.
    /// </summary>
    public int? WeightLimitGrams { get; set; }
}
