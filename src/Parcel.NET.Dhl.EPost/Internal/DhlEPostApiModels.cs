namespace Parcel.NET.Dhl.EPost.Internal;

// Wire models mirroring the E-POSTBUSINESS API v2 schema (https://api.epost.docuguide.com/swagger/v2/swagger.json).
// Property names are camelCased by the source-generated JsonContext.

/// <summary>Request body for <c>POST /api/Login</c>.</summary>
internal sealed class DhlEPostLoginRequest
{
    /// <summary>DPAG identifier of the software vendor (vendorID).</summary>
    public required string VendorID { get; set; }

    /// <summary>DPAG identifier of the software customer (EKP).</summary>
    public required string Ekp { get; set; }

    /// <summary>Security key established during password setup (SetPassword).</summary>
    public required string Secret { get; set; }

    /// <summary>Customer-defined password.</summary>
    public required string Password { get; set; }

    /// <summary>Optional partner-managed customer sub identifier.</summary>
    public string? VendorSubID { get; set; }

    /// <summary>Optional token lifetime in minutes. Maximum and default is 1440 (24h).</summary>
    public int? TokenDuration { get; set; }
}

/// <summary>Response body for <c>POST /api/Login</c>.</summary>
internal sealed class DhlEPostLoginResponse
{
    /// <summary>The JWT used as <c>Bearer</c> token for subsequent calls.</summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>A single shipment for <c>POST /api/Letter</c> (sent as an array).</summary>
internal sealed class DhlEPostLetter
{
    /// <summary>Unique file name from the source system (no special characters).</summary>
    public required string FileName { get; set; }

    /// <summary>PDF/A as Base64 string. Original PDF size must not exceed 20 MB.</summary>
    public required string Data { get; set; }

    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? AddressLine5 { get; set; }
    public required string ZipCode { get; set; }
    public required string City { get; set; }

    /// <summary>ISO 3166-1 country (German, upper case). Omit/blank for domestic.</summary>
    public string? Country { get; set; }

    public string? SenderAdressLine1 { get; set; }
    public string? SenderStreet { get; set; }
    public string? SenderZipCode { get; set; }
    public string? SenderCity { get; set; }

    public bool? IsColor { get; set; }
    public bool? IsDuplex { get; set; }

    /// <summary>Registered-mail option: "Einwurf Einschreiben", "Einschreiben", "Einschreiben Rückschein".</summary>
    public string? RegisteredLetter { get; set; }

    /// <summary>When true the shipment is treated as a test (delivered to <see cref="TestEMail"/> instead of mailed).</summary>
    public bool? TestFlag { get; set; }
    public string? TestEMail { get; set; }

    /// <summary>Enables duplicate protection against recent submissions.</summary>
    public bool? ActivateDuplicateFailsafe { get; set; }

    /// <summary>Optional grouping id (int32) for batch status queries. 0 (or omitted) means unused.</summary>
    public int? BatchID { get; set; }

    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }
    public string? Custom4 { get; set; }
    public string? Custom5 { get; set; }
}

/// <summary>Element of the response array of <c>POST /api/Letter</c>.</summary>
internal sealed class DhlEPostLetterIdent
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>Unique shipment id used for later status queries.</summary>
    public long LetterID { get; set; }
}

/// <summary>Status of a shipment (<c>GET /api/Letter/{letterID}</c>, <c>POST /api/Letter/StatusQuery</c>).</summary>
internal sealed class DhlEPostLetterStatus
{
    public long LetterID { get; set; }
    public string FileName { get; set; } = string.Empty;

    /// <summary>1 accepted, 2 processing, 3 handed to print centre, 4 dispatched, 99 error (see errorList).</summary>
    public int StatusID { get; set; }

    public DateTimeOffset? PrintFeedbackDate { get; set; }
    public int? NoOfPages { get; set; }
    public string? FrankierID { get; set; }

    public string? RegisteredLetterID { get; set; }
    public string? RegisteredLetterStatus { get; set; }
    public DateTimeOffset? RegisteredLetterStatusDate { get; set; }

    public string? Custom1 { get; set; }
    public string? Custom5 { get; set; }

    public List<DhlEPostError>? ErrorList { get; set; }
}

/// <summary>Error/info/warning entry returned by the API.</summary>
internal sealed class DhlEPostError
{
    /// <summary>"Info", "Warning" or "Error".</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>Unique message code (e.g. "E101").</summary>
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? Date { get; set; }
}
