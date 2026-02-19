namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Combined catalog response from GET /app/catalog.
/// </summary>
public class CatalogResult
{
    /// <summary>
    /// Gets or sets the available page formats for printing.
    /// </summary>
    public List<PageFormat> PageFormats { get; set; } = [];

    /// <summary>
    /// Gets or sets the contract products (product codes and prices).
    /// </summary>
    public List<ContractProduct> ContractProducts { get; set; } = [];

    /// <summary>
    /// Gets or sets public catalog items.
    /// </summary>
    public List<PublicCatalogItem> PublicCatalogItems { get; set; } = [];

    /// <summary>
    /// Gets or sets private catalog image links.
    /// </summary>
    public List<string> PrivateCatalogImageLinks { get; set; } = [];
}

/// <summary>
/// A page format for Internetmarke label printing.
/// </summary>
public class PageFormat
{
    /// <summary>
    /// Gets or sets the page format ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the format name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the format description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the page type (e.g. "REGULARPAGE", "ENVELOPE").
    /// </summary>
    public string? PageType { get; set; }

    /// <summary>
    /// Gets or sets whether address printing is possible with this format.
    /// </summary>
    public bool IsAddressPossible { get; set; }

    /// <summary>
    /// Gets or sets whether image printing is possible with this format.
    /// </summary>
    public bool IsImagePossible { get; set; }
}

/// <summary>
/// A contract product with product code and price.
/// </summary>
public class ContractProduct
{
    /// <summary>
    /// Gets or sets the product code (e.g. 10001 for Standardbrief).
    /// </summary>
    public int ProductCode { get; set; }

    /// <summary>
    /// Gets or sets the price in EUR cents.
    /// </summary>
    public int Price { get; set; }
}

/// <summary>
/// An item in the public image catalog.
/// </summary>
public class PublicCatalogItem
{
    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the category description.
    /// </summary>
    public string? CategoryDescription { get; set; }

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the image links.
    /// </summary>
    public List<string> Images { get; set; } = [];
}
