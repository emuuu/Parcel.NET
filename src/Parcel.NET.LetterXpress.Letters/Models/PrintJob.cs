namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// A single recipient item of a print job.
/// </summary>
public class PrintJobItem
{
    /// <summary>
    /// Gets the recipient address as a single line.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public int Pages { get; init; }

    /// <summary>
    /// Gets the net amount charged for this item.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the VAT for this item.
    /// </summary>
    public decimal Vat { get; init; }

    /// <summary>
    /// Gets the item status (e.g. <c>queue</c>, <c>sent</c>).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets the registered mail tracking code, if available.
    /// </summary>
    public string? TrackingCode { get; init; }

    /// <summary>
    /// Gets the rendered letter PDF as base64, returned only when retrieving a single job.
    /// </summary>
    public string? Base64Data { get; init; }
}

/// <summary>
/// A LetterXpress print job.
/// </summary>
public class PrintJob
{
    /// <summary>
    /// Gets the print job ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the shipping destination (e.g. <c>national</c>).
    /// </summary>
    public string? Shipping { get; init; }

    /// <summary>
    /// Gets the print mode (e.g. <c>simplex</c>).
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Gets the print color (<c>1</c> for black/white, <c>4</c> for color).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Gets a value indicating whether a C4 envelope is used.
    /// </summary>
    public bool C4 { get; init; }

    /// <summary>
    /// Gets the registered mail option (<c>r1</c>, <c>r2</c>), if any.
    /// </summary>
    public string? Registered { get; init; }

    /// <summary>
    /// Gets a value indicating whether a bank form is attached.
    /// </summary>
    public bool BankForm { get; init; }

    /// <summary>
    /// Gets the notice stored with the job.
    /// </summary>
    public string? Notice { get; init; }

    /// <summary>
    /// Gets the job status (e.g. <c>queue</c>, <c>hold</c>, <c>done</c>, <c>canceled</c>, <c>draft</c>).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets the dispatch date, if set.
    /// </summary>
    public string? DispatchDate { get; init; }

    /// <summary>
    /// Gets the original file name, if provided.
    /// </summary>
    public string? FilenameOriginal { get; init; }

    /// <summary>
    /// Gets the creation timestamp. The API reports times in German local time (Europe/Berlin).
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update timestamp. The API reports times in German local time (Europe/Berlin).
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Gets the recipient items of the job.
    /// </summary>
    public IReadOnlyList<PrintJobItem> Items { get; init; } = [];
}
