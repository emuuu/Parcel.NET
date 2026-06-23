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

internal sealed class LxBackgroundsWire
{
    public string? Base64BackgroundFirstPage { get; set; }
    public string? Base64BackgroundOtherPages { get; set; }
}

internal sealed class LxTermsWire
{
    public string Base64Terms { get; set; } = "";
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
    public string Base64File { get; set; } = "";
    public string Base64FileChecksum { get; set; } = "";
    public LxSpecificationWire Specification { get; set; } = new();
    public string? FilenameOriginal { get; set; }
    public string? Registered { get; set; }
    public string? DispatchDate { get; set; }
    public LxSerialWire? SerialLetter { get; set; }
    public LxEmailLetterWire? EmailLetter { get; set; }
    public List<string>? Base64Attachments { get; set; }
    public LxBackgroundsWire? Backgrounds { get; set; }
    public LxTermsWire? TermsAndConditions { get; set; }
    public LxBankFormWire? BankForm { get; set; }
    public string? Notice { get; set; }
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
    public string? Message { get; set; }
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
