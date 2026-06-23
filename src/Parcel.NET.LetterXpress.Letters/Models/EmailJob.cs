namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// A LetterXpress SMART@MAIL e-mail job.
/// </summary>
public class EmailJob
{
    /// <summary>
    /// Gets the e-mail job ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the sender e-mail address.
    /// </summary>
    public string? EmailSender { get; init; }

    /// <summary>
    /// Gets the recipient e-mail address.
    /// </summary>
    public string? EmailReceiver { get; init; }

    /// <summary>
    /// Gets the SMART@MAIL option (<c>maildirect</c>, <c>mailplus</c>, <c>mailsecure</c>).
    /// </summary>
    public string? EmailOption { get; init; }

    /// <summary>
    /// Gets the timestamp the e-mail was sent, if available.
    /// </summary>
    public DateTimeOffset? SentAt { get; init; }

    /// <summary>
    /// Gets the net amount charged.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the VAT.
    /// </summary>
    public decimal Vat { get; init; }

    /// <summary>
    /// Gets the job status (e.g. <c>queue</c>, <c>hold</c>, <c>success</c>).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets the e-mail subject.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Gets the e-mail content.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Gets the e-mail footer.
    /// </summary>
    public string? Footer { get; init; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Gets the ID of the associated print job, when the e-mail job is part of a serial response.
    /// </summary>
    public long? PrintJobId { get; init; }

    /// <summary>
    /// Gets the associated print job, returned for MailPlus jobs.
    /// </summary>
    public PrintJob? PrintJob { get; init; }
}

/// <summary>
/// Result of submitting an e-mail job. Depending on the request this contains a single e-mail job,
/// multiple e-mail jobs (serial processing), and/or print jobs created for pages without a white code.
/// </summary>
public class EmailJobResult
{
    /// <summary>
    /// Gets the created e-mail jobs.
    /// </summary>
    public IReadOnlyList<EmailJob> EmailJobs { get; init; } = [];

    /// <summary>
    /// Gets any print jobs created alongside the e-mail jobs (e.g. for MailPlus or pages without a white code).
    /// </summary>
    public IReadOnlyList<PrintJob> PrintJobs { get; init; } = [];
}
