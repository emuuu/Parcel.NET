namespace Parcel.NET.Dhl.EPost.Models;

/// <summary>
/// A single hybrid-mail shipment to submit via the E-POSTBUSINESS API.
/// A PDF/A-1b document is printed, enveloped, franked and delivered by Deutsche Post.
/// </summary>
public class EPostLetterRequest
{
    /// <summary>Unique file name from the source system (no special characters). Also used to match the result.</summary>
    public required string FileName { get; set; }

    /// <summary>The letter document as PDF/A-1b. Must not exceed 20 MB. Transmitted as Base64.</summary>
    public required byte[] PdfContent { get; set; }

    // --- Recipient ---

    /// <summary>Recipient line 1 (e.g. name / company). Required.</summary>
    public required string AddressLine1 { get; set; }

    /// <summary>Recipient line 2 (e.g. addition / c/o).</summary>
    public string? AddressLine2 { get; set; }

    /// <summary>Recipient line 3 (e.g. street + house number).</summary>
    public string? AddressLine3 { get; set; }

    /// <summary>Postal code. Required (use blanks for foreign destinations without ZIP).</summary>
    public required string ZipCode { get; set; }

    /// <summary>City. Required.</summary>
    public required string City { get; set; }

    /// <summary>ISO 3166-1 country name in German, upper case. Omit for domestic mail.</summary>
    public string? Country { get; set; }

    // --- Sender (optional) ---

    /// <summary>Sender address line 1 (e.g. company name).</summary>
    public string? SenderLine1 { get; set; }

    /// <summary>Sender street and house number.</summary>
    public string? SenderStreet { get; set; }

    /// <summary>Sender postal code.</summary>
    public string? SenderZipCode { get; set; }

    /// <summary>Sender city.</summary>
    public string? SenderCity { get; set; }

    // --- Print / product options ---

    /// <summary>Print in color (default: grayscale).</summary>
    public bool Color { get; set; }

    /// <summary>Print double-sided.</summary>
    public bool Duplex { get; set; }

    /// <summary>Registered-mail option. Default <see cref="RegisteredLetterType.None"/>.</summary>
    public RegisteredLetterType RegisteredLetter { get; set; } = RegisteredLetterType.None;

    /// <summary>Reject the shipment if an identical one was submitted recently.</summary>
    public bool ActivateDuplicateFailsafe { get; set; }

    // --- Test mode ---

    /// <summary>When true the shipment is not mailed but sent to <see cref="TestEMail"/> for review.</summary>
    public bool Test { get; set; }

    /// <summary>Recipient e-mail for test shipments.</summary>
    public string? TestEMail { get; set; }

    // --- References ---

    /// <summary>Optional numeric grouping id (int32) to query a whole batch's status at once.</summary>
    public int? BatchId { get; set; }

    /// <summary>Free reference field, e.g. your own shipment or invoice id (queryable via Custom1).</summary>
    public string? Custom1 { get; set; }
}
