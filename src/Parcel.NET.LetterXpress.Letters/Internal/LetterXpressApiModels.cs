using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Parcel.NET.LetterXpress.Letters.Internal;

// All property names are mapped to snake_case via the JsonContext naming policy,
// except where an explicit [JsonPropertyName] overrides it (e.g. "apikey").

internal sealed class LxAuth
{
    public string Username { get; set; } = "";

    [JsonPropertyName("apikey")]
    public string ApiKey { get; set; } = "";

    public string Mode { get; set; } = "";
}

internal sealed class LxAuthRequest
{
    public LxAuth Auth { get; set; } = new();
}

internal sealed class LxSpecificationWire
{
    public string? Color { get; set; }
    public string? Mode { get; set; }
    public string? Shipping { get; set; }
    public int? C4 { get; set; }
    public int? Pages { get; set; }
    public string? EmailOption { get; set; }
}

internal sealed class LxSerialWire
{
    public string PagesSeparatorType { get; set; } = "";

    // Either a JSON string (keyword) or a JSON number (page count), depending on the separator type.
    public JsonNode? PagesSeparatorValue { get; set; }
}

internal sealed class LxEmailLetterWire
{
    public string EmailOption { get; set; } = "";
    public string EmailReceiver { get; set; } = "";
}

// The PDF payloads below are kept as raw bytes: System.Text.Json writes a byte[] as a Base64 JSON string directly
// into its output buffer (Utf8JsonWriter.WriteBase64StringValue), so no intermediate Base64 string is ever created.
internal sealed class LxBackgroundsWire
{
    public byte[]? Base64BackgroundFirstPage { get; set; }
    public byte[]? Base64BackgroundOtherPages { get; set; }
}

internal sealed class LxTermsWire
{
    public byte[] Base64Terms { get; set; } = [];
    public int TermsOnAllPages { get; set; }
}

internal sealed class LxBankFormWire
{
    public int BankFormIncluded { get; set; }
    public string? Payee { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? Amount { get; set; }
    public string? PurposeOfPayment { get; set; }
    public string? PurposeOfPayment2 { get; set; }
}

internal sealed class LxLetterWire
{
    public byte[] Base64File { get; set; } = [];

    // MD5 (lower-case hex) over the ASCII bytes of the Base64 representation of Base64File.
    public string Base64FileChecksum { get; set; } = "";
    public LxSpecificationWire Specification { get; set; } = new();
    public string? FilenameOriginal { get; set; }
    public string? Registered { get; set; }
    public string? DispatchDate { get; set; }
    public LxSerialWire? SerialLetter { get; set; }
    public LxEmailLetterWire? EmailLetter { get; set; }
    public List<byte[]>? Base64Attachments { get; set; }
    public LxBackgroundsWire? Backgrounds { get; set; }
    public LxTermsWire? TermsAndConditions { get; set; }
    public LxBankFormWire? BankForm { get; set; }
    public string? Notice { get; set; }

    /// <summary>
    /// Returns a shallow copy in which every PDF payload is replaced by an empty array (same presence/count as the
    /// original), plus the number of Base64 characters the original payloads add to the serialized JSON.
    /// Serializing the copy therefore yields exactly the original JSON minus the Base64 characters.
    /// </summary>
    public LxLetterWire CopyWithoutPayloads(out long base64Length)
    {
        long total = Base64Length(Base64File);
        var copy = (LxLetterWire)MemberwiseClone();
        copy.Base64File = [];

        if (Base64Attachments is { } attachments)
        {
            var empty = new List<byte[]>(attachments.Count);
            foreach (var attachment in attachments)
            {
                total += Base64Length(attachment);
                empty.Add([]);
            }

            copy.Base64Attachments = empty;
        }

        if (Backgrounds is { } backgrounds)
        {
            total += Base64Length(backgrounds.Base64BackgroundFirstPage) + Base64Length(backgrounds.Base64BackgroundOtherPages);
            copy.Backgrounds = new LxBackgroundsWire
            {
                Base64BackgroundFirstPage = backgrounds.Base64BackgroundFirstPage is null ? null : [],
                Base64BackgroundOtherPages = backgrounds.Base64BackgroundOtherPages is null ? null : []
            };
        }

        if (TermsAndConditions is { } terms)
        {
            total += Base64Length(terms.Base64Terms);
            copy.TermsAndConditions = new LxTermsWire { Base64Terms = [], TermsOnAllPages = terms.TermsOnAllPages };
        }

        base64Length = total;
        return copy;
    }

    // Padded Base64 length of a payload: 4 * ceil(n / 3). Base64 characters are never JSON-escaped.
    private static long Base64Length(byte[]? payload) => payload is null ? 0 : (payload.LongLength + 2) / 3 * 4;
}

internal sealed class LxLetterRequest
{
    public LxAuth Auth { get; set; } = new();
    public LxLetterWire Letter { get; set; } = new();
}

internal sealed class LxPriceLetterWire
{
    public LxSpecificationWire Specification { get; set; } = new();
    public string? Registered { get; set; }
}

internal sealed class LxPriceRequest
{
    public LxAuth Auth { get; set; } = new();
    public LxPriceLetterWire Letter { get; set; } = new();
}

internal sealed class LxUpdateLetterWire
{
    public string? DispatchDate { get; set; }
    public string? Registered { get; set; }
    public string? Notice { get; set; }
    public LxSpecificationWire? Specification { get; set; }
}

internal sealed class LxUpdateRequest
{
    public LxAuth Auth { get; set; } = new();
    public LxUpdateLetterWire Letter { get; set; } = new();
}

internal sealed class LxEmailWire
{
    public string EmailReceiver { get; set; } = "";
}

internal sealed class LxEmailUpdateRequest
{
    public LxAuth Auth { get; set; } = new();
    public LxEmailWire Email { get; set; } = new();
}

// ---- Response envelopes -------------------------------------------------

internal sealed class LxResponse<T>
{
    public int Status { get; set; }

    // "message" is deliberately not mapped: it is a string or, for validation errors, an object, and is read via
    // LetterXpressErrorHelper instead so that both shapes deserialize.
    public T? Data { get; set; }
}

internal sealed class LxBalanceData
{
    public decimal Balance { get; set; }
    public string? Currency { get; set; }
}

internal sealed class LxPriceEcho
{
    public LxSpecificationWire? Specification { get; set; }
}

internal sealed class LxPriceData
{
    public decimal Price { get; set; }
    public LxPriceEcho? Letter { get; set; }
}

internal sealed class LxPrintJobItemWire
{
    public string? Address { get; set; }
    public int Pages { get; set; }
    public decimal Amount { get; set; }
    public decimal Vat { get; set; }
    public string? Status { get; set; }
    public string? TrackingCode { get; set; }
    public string? Base64Data { get; set; }
}

internal sealed class LxPrintJobWire
{
    public long Id { get; set; }
    public string? Shipping { get; set; }
    public string? Mode { get; set; }
    public string? Color { get; set; }
    public int C4 { get; set; }
    public string? Registered { get; set; }
    public int BankForm { get; set; }
    public string? Notice { get; set; }
    public string? Status { get; set; }
    public string? DispatchDate { get; set; }
    public string? FilenameOriginal { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
    public List<LxPrintJobItemWire>? Items { get; set; }
}

internal sealed class LxPaginationWire
{
    public int Total { get; set; }
    public int Count { get; set; }
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int PerPage { get; set; }
    public string? FirstPageUrl { get; set; }
    public string? LastPageUrl { get; set; }
    public string? NextPageUrl { get; set; }
    public string? PrevPageUrl { get; set; }
}

internal sealed class LxPrintJobsData
{
    [JsonPropertyName("printjobs")]
    public List<LxPrintJobWire>? PrintJobs { get; set; }

    public LxPaginationWire? Pagination { get; set; }
}

internal sealed class LxEmailJobWire
{
    public long Id { get; set; }
    public string? EmailSender { get; set; }
    public string? EmailReceiver { get; set; }
    public string? EmailOption { get; set; }
    public string? SentAt { get; set; }
    public decimal Amount { get; set; }
    public decimal Vat { get; set; }
    public string? Status { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public string? Footer { get; set; }
    public string? CreatedAt { get; set; }
    public long? PrintjobId { get; set; }
    public LxPrintJobWire? Printjob { get; set; }
}

internal sealed class LxEmailCreateData
{
    [JsonPropertyName("printjobs")]
    public List<LxPrintJobWire>? PrintJobs { get; set; }

    [JsonPropertyName("emailjobs")]
    public List<LxEmailJobWire>? EmailJobs { get; set; }
}

internal sealed class LxTransactionWire
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Description { get; set; }
    public string? CreatedAt { get; set; }
}

internal sealed class LxTransactionsData
{
    public List<LxTransactionWire>? Transactions { get; set; }
    public LxPaginationWire? Pagination { get; set; }
}

internal sealed class LxInvoiceWire
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Vat { get; set; }
    public string? InvoiceDate { get; set; }
    public string? Base64Data { get; set; }
}

internal sealed class LxInvoicesData
{
    public List<LxInvoiceWire>? Invoices { get; set; }
    public LxPaginationWire? Pagination { get; set; }
}
