namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// Request for submitting a letter (print job or e-mail job). The PDF is supplied as raw bytes;
/// the client computes the base64 encoding and MD5 checksum automatically.
/// </summary>
public class LetterRequest
{
    /// <summary>
    /// Gets or sets the letter PDF as raw bytes. Max. 50 MB.
    /// </summary>
    public required byte[] File { get; set; }

    /// <summary>
    /// Gets or sets the print and shipping specification.
    /// </summary>
    public required LetterSpecification Specification { get; set; }

    /// <summary>
    /// Gets or sets the original file name (e.g. <c>Rechnung-123456.pdf</c>).
    /// Optional; necessary for LXP SMART@MAIL and returned when retrieving jobs.
    /// </summary>
    public string? FilenameOriginal { get; set; }

    /// <summary>
    /// Gets or sets the registered mail option. Registered mail can only be sent nationally.
    /// Defaults to <see cref="RegisteredMail.None"/>.
    /// </summary>
    public RegisteredMail Registered { get; set; } = RegisteredMail.None;

    /// <summary>
    /// Gets or sets the dispatch date. Optional; must be in the future.
    /// </summary>
    public DateOnly? DispatchDate { get; set; }

    /// <summary>
    /// Gets or sets serial letter options. Cannot be combined with <see cref="EmailLetter"/>.
    /// </summary>
    public SerialLetterOptions? SerialLetter { get; set; }

    /// <summary>
    /// Gets or sets LXP SMART@MAIL e-mail options for a single job. Cannot be combined with <see cref="SerialLetter"/>.
    /// </summary>
    public EmailLetterOptions? EmailLetter { get; set; }

    /// <summary>
    /// Gets or sets additional attachments as raw PDF bytes. Each attachment must be A4 portrait
    /// (29.7 x 21.0 cm). The order is preserved.
    /// </summary>
    public IReadOnlyList<byte[]>? Attachments { get; set; }

    /// <summary>
    /// Gets or sets the letter background PDFs.
    /// </summary>
    public LetterBackgrounds? Backgrounds { get; set; }

    /// <summary>
    /// Gets or sets the terms and conditions PDF.
    /// </summary>
    public TermsAndConditions? TermsAndConditions { get; set; }

    /// <summary>
    /// Gets or sets the bank transfer form.
    /// </summary>
    public BankForm? BankForm { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary notice stored with the job. Max. 255 characters.
    /// </summary>
    public string? Notice { get; set; }
}

/// <summary>
/// Request for changing an existing print job within the first 15 minutes after submission.
/// Only the specification and shipping options can be changed; the PDF cannot.
/// All properties are optional — only set ones are sent.
/// </summary>
public class PrintJobUpdate
{
    /// <summary>
    /// Gets or sets the new dispatch date.
    /// </summary>
    public DateOnly? DispatchDate { get; set; }

    /// <summary>
    /// Gets or sets the new registered mail option.
    /// </summary>
    public RegisteredMail? Registered { get; set; }

    /// <summary>
    /// Gets or sets the new notice.
    /// </summary>
    public string? Notice { get; set; }

    /// <summary>
    /// Gets or sets the new print color.
    /// </summary>
    public LetterColor? Color { get; set; }

    /// <summary>
    /// Gets or sets the new print mode.
    /// </summary>
    public PrintMode? Mode { get; set; }

    /// <summary>
    /// Gets or sets the new shipping destination.
    /// </summary>
    public ShippingType? Shipping { get; set; }

    /// <summary>
    /// Gets or sets the new C4 envelope flag.
    /// </summary>
    public bool? C4 { get; set; }
}

/// <summary>
/// Request for a price calculation.
/// </summary>
public class PriceRequest
{
    /// <summary>
    /// Gets or sets the number of pages.
    /// </summary>
    public required int Pages { get; set; }

    /// <summary>
    /// Gets or sets the print color. Defaults to <see cref="LetterColor.BlackWhite"/>.
    /// </summary>
    public LetterColor Color { get; set; } = LetterColor.BlackWhite;

    /// <summary>
    /// Gets or sets the print mode. Defaults to <see cref="PrintMode.Simplex"/>.
    /// </summary>
    public PrintMode Mode { get; set; } = PrintMode.Simplex;

    /// <summary>
    /// Gets or sets the shipping destination. Only <see cref="ShippingType.National"/> and
    /// <see cref="ShippingType.International"/> are valid for price requests.
    /// </summary>
    public ShippingType Shipping { get; set; } = ShippingType.National;

    /// <summary>
    /// Gets or sets a value indicating whether to use a C4 envelope.
    /// </summary>
    public bool C4 { get; set; }

    /// <summary>
    /// Gets or sets the SMART@MAIL e-mail option. Optional.
    /// </summary>
    public EmailOption? EmailOption { get; set; }

    /// <summary>
    /// Gets or sets the registered mail option. Optional; only national shipping is supported for registered mail.
    /// </summary>
    public RegisteredMail Registered { get; set; } = RegisteredMail.None;
}
