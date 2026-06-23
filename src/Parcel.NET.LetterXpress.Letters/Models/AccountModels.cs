namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// Account balance.
/// </summary>
public class Balance
{
    /// <summary>
    /// Gets the current balance.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the currency (e.g. <c>EUR</c>).
    /// </summary>
    public string? Currency { get; init; }
}

/// <summary>
/// Result of a price calculation.
/// </summary>
public class PriceResult
{
    /// <summary>
    /// Gets the calculated gross price.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the number of pages the price was calculated for.
    /// </summary>
    public int? Pages { get; init; }

    /// <summary>
    /// Gets the print color the price was calculated for.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Gets the print mode the price was calculated for.
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Gets the shipping destination the price was calculated for.
    /// </summary>
    public string? Shipping { get; init; }
}

/// <summary>
/// An account transaction.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets the amount (negative for charges, positive for top-ups).
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the currency (e.g. <c>EUR</c>).
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }
}

/// <summary>
/// An invoice.
/// </summary>
public class Invoice
{
    /// <summary>
    /// Gets the invoice ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the net amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the VAT.
    /// </summary>
    public decimal Vat { get; init; }

    /// <summary>
    /// Gets the invoice date.
    /// </summary>
    public DateOnly? InvoiceDate { get; init; }

    /// <summary>
    /// Gets the invoice PDF as base64, returned only when retrieving a single invoice.
    /// </summary>
    public string? Base64Data { get; init; }
}

/// <summary>
/// Pagination metadata returned by list endpoints.
/// </summary>
public class Pagination
{
    /// <summary>Gets the total number of items.</summary>
    public int Total { get; init; }

    /// <summary>Gets the number of items on the current page.</summary>
    public int Count { get; init; }

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage { get; init; }

    /// <summary>Gets the last page number.</summary>
    public int LastPage { get; init; }

    /// <summary>Gets the number of items per page.</summary>
    public int PerPage { get; init; }

    /// <summary>Gets the URL of the first page, if any.</summary>
    public string? FirstPageUrl { get; init; }

    /// <summary>Gets the URL of the last page, if any.</summary>
    public string? LastPageUrl { get; init; }

    /// <summary>Gets the URL of the next page, if any.</summary>
    public string? NextPageUrl { get; init; }

    /// <summary>Gets the URL of the previous page, if any.</summary>
    public string? PrevPageUrl { get; init; }
}

/// <summary>
/// A paged list result.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Gets the items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the pagination metadata, if available.
    /// </summary>
    public Pagination? Pagination { get; init; }
}
