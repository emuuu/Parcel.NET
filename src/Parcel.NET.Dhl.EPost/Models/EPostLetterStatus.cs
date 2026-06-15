namespace Parcel.NET.Dhl.EPost.Models;

/// <summary>Processing status of an E-POST hybrid-mail shipment.</summary>
public class EPostLetterStatus
{
    /// <summary>The unique shipment id.</summary>
    public long LetterId { get; set; }

    /// <summary>The file name supplied at submission.</summary>
    public required string FileName { get; set; }

    /// <summary>The processing stage. See <see cref="EPostLetterStage"/>.</summary>
    public EPostLetterStage Stage { get; set; }

    /// <summary>The raw status id returned by the API (1, 2, 3, 4 or 99).</summary>
    public int RawStatusId { get; set; }

    /// <summary>Number of printed pages, once known.</summary>
    public int? PageCount { get; set; }

    /// <summary>Registered-mail tracking number, if applicable.</summary>
    public string? RegisteredLetterId { get; set; }

    /// <summary>Your reference echoed back (Custom1).</summary>
    public string? Custom1 { get; set; }

    /// <summary>Diagnostic messages. Populated (with level "Error") when <see cref="Stage"/> is <see cref="EPostLetterStage.Error"/>.</summary>
    public IReadOnlyList<EPostMessage> Messages { get; set; } = [];
}

/// <summary>The lifecycle stages reported by E-POST (status id semantics).</summary>
public enum EPostLetterStage
{
    /// <summary>Unknown / not mapped.</summary>
    Unknown = 0,

    /// <summary>1 — accepted (valid JSON/schema).</summary>
    Accepted = 1,

    /// <summary>2 — processing (PDF validated for E-POST conformity).</summary>
    Processing = 2,

    /// <summary>3 — handed over to the print centre.</summary>
    HandedToPrintCentre = 3,

    /// <summary>4 — reported as dispatched by the print centre.</summary>
    Dispatched = 4,

    /// <summary>99 — processing error (see <see cref="EPostLetterStatus.Messages"/>).</summary>
    Error = 99
}

/// <summary>A diagnostic message attached to a shipment status.</summary>
public class EPostMessage
{
    /// <summary>"Info", "Warning" or "Error".</summary>
    public required string Level { get; set; }

    /// <summary>Unique message code (e.g. "E101").</summary>
    public required string Code { get; set; }

    /// <summary>Human-readable description.</summary>
    public required string Description { get; set; }
}
