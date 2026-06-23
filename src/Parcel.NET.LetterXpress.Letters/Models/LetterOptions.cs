namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// Options for splitting a serial letter into individual letters.
/// </summary>
public class SerialLetterOptions
{
    /// <summary>
    /// Gets or sets the separator type.
    /// </summary>
    public SeparatorType PagesSeparatorType { get; set; }

    /// <summary>
    /// Gets or sets the separator value: a keyword when <see cref="PagesSeparatorType"/> is
    /// <see cref="SeparatorType.String"/>, or a page count when it is <see cref="SeparatorType.Number"/>.
    /// </summary>
    public required string PagesSeparatorValue { get; set; }
}

/// <summary>
/// LXP SMART@MAIL options for transmitting a single job as an e-mail letter.
/// Cannot be combined with a serial letter.
/// </summary>
public class EmailLetterOptions
{
    /// <summary>
    /// Gets or sets the SMART@MAIL delivery option.
    /// </summary>
    public EmailOption EmailOption { get; set; }

    /// <summary>
    /// Gets or sets the valid recipient e-mail address.
    /// </summary>
    public required string EmailReceiver { get; set; }
}

/// <summary>
/// Background PDFs inserted behind a letter's pages. Each background must be a single-page
/// A4 portrait (29.7 x 21.0 cm) PDF.
/// </summary>
public class LetterBackgrounds
{
    /// <summary>
    /// Gets or sets the background PDF for the first page.
    /// </summary>
    public byte[]? FirstPage { get; set; }

    /// <summary>
    /// Gets or sets the background PDF for all other pages.
    /// </summary>
    public byte[]? OtherPages { get; set; }
}

/// <summary>
/// Terms and conditions PDF appended to a letter. Must be a single-page A4 portrait
/// (29.7 x 21.0 cm) PDF. Printing is automatically double-sided.
/// </summary>
public class TermsAndConditions
{
    /// <summary>
    /// Gets or sets the terms and conditions PDF.
    /// </summary>
    public required byte[] Terms { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the terms are inserted behind all pages
    /// (<see langword="true"/>) or only behind the first page (<see langword="false"/>).
    /// </summary>
    public bool OnAllPages { get; set; }
}

/// <summary>
/// Bank transfer form attached as the last sheet of a letter.
/// </summary>
public class BankForm
{
    /// <summary>
    /// Gets or sets a value indicating whether the bank form is already included in the PDF's last sheet.
    /// When <see langword="true"/>, the payment detail fields below are ignored and the last (blank) sheet is used.
    /// </summary>
    public bool Included { get; set; }

    /// <summary>
    /// Gets or sets the payee. Max. 27 characters. Ignored when <see cref="Included"/> is <see langword="true"/>.
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// Gets or sets the IBAN. Max. 34 characters.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the BIC. Max. 11 characters.
    /// </summary>
    public string? Bic { get; set; }

    /// <summary>
    /// Gets or sets the amount. Max. 12 characters.
    /// </summary>
    public string? Amount { get; set; }

    /// <summary>
    /// Gets or sets the purpose of payment. Max. 27 characters.
    /// </summary>
    public string? PurposeOfPayment { get; set; }

    /// <summary>
    /// Gets or sets the second purpose of payment line. Max. 27 characters.
    /// </summary>
    public string? PurposeOfPayment2 { get; set; }
}
