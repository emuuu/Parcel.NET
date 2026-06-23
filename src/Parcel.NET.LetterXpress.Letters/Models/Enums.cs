namespace Parcel.NET.LetterXpress.Letters.Models;

/// <summary>
/// Print color of a letter.
/// </summary>
public enum LetterColor
{
    /// <summary>Black/white printing (API value <c>"1"</c>).</summary>
    BlackWhite,

    /// <summary>Color printing (API value <c>"4"</c>).</summary>
    Color
}

/// <summary>
/// Print mode of a letter.
/// </summary>
public enum PrintMode
{
    /// <summary>Single-sided printing (API value <c>"simplex"</c>).</summary>
    Simplex,

    /// <summary>Double-sided printing (API value <c>"duplex"</c>).</summary>
    Duplex
}

/// <summary>
/// Shipping destination of a letter.
/// </summary>
public enum ShippingType
{
    /// <summary>National shipping (API value <c>"national"</c>).</summary>
    National,

    /// <summary>International shipping (API value <c>"international"</c>).</summary>
    International,

    /// <summary>Automatic detection (API value <c>"auto"</c>). Not valid for price requests.</summary>
    Auto
}

/// <summary>
/// Registered mail option. Registered mail can only be sent nationally.
/// </summary>
public enum RegisteredMail
{
    /// <summary>No registered mail.</summary>
    None,

    /// <summary>Einschreiben Einwurf (API value <c>"r1"</c>).</summary>
    EinschreibenEinwurf,

    /// <summary>Einschreiben (API value <c>"r2"</c>).</summary>
    Einschreiben
}

/// <summary>
/// LXP SMART@MAIL e-mail delivery option.
/// </summary>
public enum EmailOption
{
    /// <summary>maildirect.</summary>
    MailDirect,

    /// <summary>mailplus.</summary>
    MailPlus,

    /// <summary>mailsecure.</summary>
    MailSecure
}

/// <summary>
/// Type of the serial letter page separator.
/// </summary>
public enum SeparatorType
{
    /// <summary>A keyword string is used to separate the individual letters (API value <c>"string"</c>).</summary>
    String,

    /// <summary>A fixed number of pages is used to separate the individual letters (API value <c>"number"</c>).</summary>
    Number
}

/// <summary>
/// Filter for listing print jobs.
/// </summary>
public enum PrintJobFilter
{
    /// <summary>Jobs in the queue.</summary>
    Queue,

    /// <summary>Held jobs.</summary>
    Hold,

    /// <summary>Processed jobs.</summary>
    Done,

    /// <summary>Cancelled jobs.</summary>
    Canceled,

    /// <summary>Jobs in the Postbox.</summary>
    Draft
}

/// <summary>
/// Filter for listing e-mail jobs. Accepted values were verified against the live API
/// (the e-mail equivalent of the print job's <c>done</c> status is <c>success</c>).
/// </summary>
public enum EmailJobFilter
{
    /// <summary>Jobs in the queue.</summary>
    Queue,

    /// <summary>Held jobs.</summary>
    Hold,

    /// <summary>Cancelled jobs.</summary>
    Canceled,

    /// <summary>Jobs in the Postbox.</summary>
    Draft,

    /// <summary>Successfully sent jobs.</summary>
    Success
}

/// <summary>
/// Filter for listing transactions.
/// </summary>
public enum TransactionFilter
{
    /// <summary>Pay-ins / vouchers / kickbacks.</summary>
    PayIns,

    /// <summary>Pay-outs.</summary>
    PayOuts,

    /// <summary>Print jobs.</summary>
    PrintJobs
}
