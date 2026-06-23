using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Parcel.NET.LetterXpress.Internal;
using Parcel.NET.LetterXpress.Letters.Internal;
using Parcel.NET.LetterXpress.Letters.Models;

namespace Parcel.NET.LetterXpress.Letters;

/// <summary>
/// LetterXpress (A&amp;O Fischer) API v3 client implementing <see cref="ILetterXpressClient"/>.
/// </summary>
public class LetterXpressClient : ILetterXpressClient
{
    private readonly HttpClient _httpClient;
    private readonly LetterXpressOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="LetterXpressClient"/>.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client for LetterXpress API requests.</param>
    /// <param name="options">LetterXpress configuration options.</param>
    public LetterXpressClient(HttpClient httpClient, IOptions<LetterXpressOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Balance> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Get, "balance", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxBalanceData, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress balance response contained no data.");
        return new Balance { Amount = data.Balance, Currency = data.Currency };
    }

    /// <inheritdoc />
    public async Task<PriceResult> GetPriceAsync(PriceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Pages <= 0)
        {
            throw new ArgumentException("Price requests require a positive page count.", nameof(request));
        }

        if (request.Shipping == ShippingType.Auto)
        {
            throw new ArgumentException(
                "Price requests support only National or International shipping, not Auto.", nameof(request));
        }

        if (request.Registered != RegisteredMail.None && request.Shipping == ShippingType.International)
        {
            throw new ArgumentException(
                "Registered mail can only be sent nationally.", nameof(request));
        }

        var payload = new LxPriceRequest
        {
            Auth = BuildAuth(),
            Letter = new LxPriceLetterWire
            {
                Specification = new LxSpecificationWire
                {
                    Pages = request.Pages,
                    Color = ColorToWire(request.Color),
                    Mode = ModeToWire(request.Mode),
                    Shipping = ShippingToWire(request.Shipping),
                    C4 = request.C4 ? 1 : 0,
                    EmailOption = request.EmailOption is { } eo ? EmailOptionToWire(eo) : null
                },
                Registered = RegisteredToWire(request.Registered)
            }
        };

        var response = await SendAsync(
            HttpMethod.Get, "price",
            JsonContent.Create(payload, LetterXpressJsonContext.Default.LxPriceRequest),
            LetterXpressJsonContext.Default.LxResponseLxPriceData, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress price response contained no data.");
        var spec = data.Letter?.Specification;
        return new PriceResult
        {
            Price = data.Price,
            Pages = spec?.Pages,
            Color = spec?.Color,
            Mode = spec?.Mode,
            Shipping = spec?.Shipping
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<PrintJob>> ListPrintJobsAsync(
        PrintJobFilter? filter = null, int? page = null, CancellationToken cancellationToken = default)
    {
        var url = BuildListUrl("printjobs", filter is { } f ? PrintJobFilterToWire(f) : null, page);

        var response = await SendAsync(
            HttpMethod.Get, url, AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxPrintJobsData, cancellationToken).ConfigureAwait(false);

        var data = response.Data;
        return new PagedResult<PrintJob>
        {
            Items = data?.PrintJobs?.Select(MapPrintJob).ToList() ?? [],
            Pagination = MapPagination(data?.Pagination)
        };
    }

    /// <inheritdoc />
    public async Task<PrintJob> CreatePrintJobAsync(LetterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLetterRequest(request);

        var payload = new LxLetterRequest { Auth = BuildAuth(), Letter = BuildLetterWire(request) };

        var response = await SendAsync(
            HttpMethod.Post, "printjobs",
            JsonContent.Create(payload, LetterXpressJsonContext.Default.LxLetterRequest),
            LetterXpressJsonContext.Default.LxResponseLxPrintJobWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress print job response contained no data.");
        return MapPrintJob(data);
    }

    /// <inheritdoc />
    public async Task<PrintJob> GetPrintJobAsync(long id, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Get, $"printjobs/{id}", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxPrintJobWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress print job response contained no data.");
        return MapPrintJob(data);
    }

    /// <inheritdoc />
    public async Task<PrintJob> UpdatePrintJobAsync(long id, PrintJobUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        LxSpecificationWire? spec = null;
        if (update.Color is not null || update.Mode is not null || update.Shipping is not null || update.C4 is not null)
        {
            spec = new LxSpecificationWire
            {
                Color = update.Color is { } c ? ColorToWire(c) : null,
                Mode = update.Mode is { } m ? ModeToWire(m) : null,
                Shipping = update.Shipping is { } s ? ShippingToWire(s) : null,
                C4 = update.C4 is { } c4 ? (c4 ? 1 : 0) : null
            };
        }

        var payload = new LxUpdateRequest
        {
            Auth = BuildAuth(),
            Letter = new LxUpdateLetterWire
            {
                DispatchDate = update.DispatchDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Registered = update.Registered is { } r ? RegisteredToWire(r) : null,
                Notice = update.Notice,
                Specification = spec
            }
        };

        var response = await SendAsync(
            HttpMethod.Put, $"printjobs/{id}",
            JsonContent.Create(payload, LetterXpressJsonContext.Default.LxUpdateRequest),
            LetterXpressJsonContext.Default.LxResponseLxPrintJobWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress print job response contained no data.");
        return MapPrintJob(data);
    }

    /// <inheritdoc />
    public async Task DeletePrintJobAsync(long id, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            HttpMethod.Delete, $"printjobs/{id}", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseJsonElement, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<EmailJobResult> CreateEmailJobAsync(LetterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLetterRequest(request);

        var payload = new LxLetterRequest { Auth = BuildAuth(), Letter = BuildLetterWire(request) };

        var response = await SendAsync(
            HttpMethod.Post, "emailjobs",
            JsonContent.Create(payload, LetterXpressJsonContext.Default.LxLetterRequest),
            LetterXpressJsonContext.Default.LxResponseJsonElement, cancellationToken).ConfigureAwait(false);

        try
        {
            return ParseEmailCreate(response.Data);
        }
        catch (JsonException ex)
        {
            throw new LetterXpressException(
                "Failed to parse LetterXpress e-mail job response.", null, null, RawTextOrNull(response.Data), ex);
        }
    }

    /// <inheritdoc />
    public async Task<EmailJob> GetEmailJobAsync(long id, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Get, $"emailjobs/{id}", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxEmailJobWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress e-mail job response contained no data.");
        return MapEmailJob(data);
    }

    /// <inheritdoc />
    public async Task<EmailJob> UpdateEmailJobAsync(long id, string emailReceiver, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailReceiver);

        var payload = new LxEmailUpdateRequest
        {
            Auth = BuildAuth(),
            Email = new LxEmailWire { EmailReceiver = emailReceiver }
        };

        var response = await SendAsync(
            HttpMethod.Put, $"emailjobs/{id}",
            JsonContent.Create(payload, LetterXpressJsonContext.Default.LxEmailUpdateRequest),
            LetterXpressJsonContext.Default.LxResponseLxEmailJobWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress e-mail job response contained no data.");
        return MapEmailJob(data);
    }

    /// <inheritdoc />
    public async Task DeleteEmailJobAsync(long id, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            HttpMethod.Delete, $"emailjobs/{id}", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseJsonElement, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<EmailJob>> ListEmailJobsAsync(
        EmailJobFilter? filter = null, int? page = null, CancellationToken cancellationToken = default)
    {
        var url = BuildListUrl("emailjobs", filter is { } f ? EmailJobFilterToWire(f) : null, page);

        var response = await SendAsync(
            HttpMethod.Get, url, AuthBody(),
            LetterXpressJsonContext.Default.LxResponseJsonElement, cancellationToken).ConfigureAwait(false);

        try
        {
            return ParseEmailList(response.Data);
        }
        catch (JsonException ex)
        {
            throw new LetterXpressException(
                "Failed to parse LetterXpress e-mail job list response.", null, null, RawTextOrNull(response.Data), ex);
        }
    }

    /// <inheritdoc />
    public async Task<PagedResult<Transaction>> ListTransactionsAsync(
        TransactionFilter? filter = null, int? page = null, CancellationToken cancellationToken = default)
    {
        var url = BuildListUrl("transactions", filter is { } f ? TransactionFilterToWire(f) : null, page);

        var response = await SendAsync(
            HttpMethod.Get, url, AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxTransactionsData, cancellationToken).ConfigureAwait(false);

        var data = response.Data;
        return new PagedResult<Transaction>
        {
            Items = data?.Transactions?.Select(MapTransaction).ToList() ?? [],
            Pagination = MapPagination(data?.Pagination)
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<Invoice>> ListInvoicesAsync(int? page = null, CancellationToken cancellationToken = default)
    {
        var url = BuildListUrl("invoices", null, page);

        var response = await SendAsync(
            HttpMethod.Get, url, AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxInvoicesData, cancellationToken).ConfigureAwait(false);

        var data = response.Data;
        return new PagedResult<Invoice>
        {
            Items = data?.Invoices?.Select(MapInvoice).ToList() ?? [],
            Pagination = MapPagination(data?.Pagination)
        };
    }

    /// <inheritdoc />
    public async Task<Invoice> GetInvoiceAsync(long id, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Get, $"invoices/{id}", AuthBody(),
            LetterXpressJsonContext.Default.LxResponseLxInvoiceWire, cancellationToken).ConfigureAwait(false);

        var data = response.Data ?? throw new LetterXpressException("LetterXpress invoice response contained no data.");
        return MapInvoice(data);
    }

    // ---- HTTP plumbing --------------------------------------------------

    private LxAuth BuildAuth() => new()
    {
        Username = _options.Username,
        ApiKey = _options.ApiKey,
        Mode = _options.Mode
    };

    private HttpContent AuthBody() =>
        JsonContent.Create(new LxAuthRequest { Auth = BuildAuth() }, LetterXpressJsonContext.Default.LxAuthRequest);

    private async Task<LxResponse<TData>> SendAsync<TData>(
        HttpMethod method,
        string url,
        HttpContent content,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<LxResponse<TData>> responseInfo,
        CancellationToken cancellationToken)
    {
        // LetterXpress requires the Content-Type to be exactly "application/json".
        // JsonContent/StringContent would otherwise append "; charset=utf-8", which the API rejects with HTTP 400.
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var request = new HttpRequestMessage(method, url) { Content = content };
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var detail = LetterXpressErrorHelper.TryParseErrorDetail(rawBody);
            throw new LetterXpressException(
                $"LetterXpress API returned {(int)response.StatusCode}: {detail}",
                response.StatusCode,
                ((int)response.StatusCode).ToString(),
                rawBody);
        }

        LxResponse<TData>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize(rawBody, responseInfo);
        }
        catch (JsonException ex)
        {
            throw new LetterXpressException(
                "Failed to deserialize LetterXpress response.", null, null, rawBody, ex);
        }

        if (parsed is null)
        {
            throw new LetterXpressException(
                "LetterXpress response was empty.", null, null, rawBody);
        }

        // The API also reports application-level errors via the body "status" field,
        // which mirrors the documented HTTP status codes (400/401/403/...).
        if (parsed.Status is not 0 and not 200)
        {
            var statusCode = Enum.IsDefined(typeof(HttpStatusCode), parsed.Status)
                ? (HttpStatusCode?)parsed.Status
                : null;
            throw new LetterXpressException(
                $"LetterXpress API returned status {parsed.Status}: {parsed.Message}",
                statusCode,
                parsed.Status.ToString(CultureInfo.InvariantCulture),
                rawBody);
        }

        return parsed;
    }

    private static string BuildListUrl(string path, string? filter, int? page)
    {
        var query = new List<string>();
        if (filter is not null) query.Add($"filter={Uri.EscapeDataString(filter)}");
        if (page is { } p) query.Add($"page={p.ToString(CultureInfo.InvariantCulture)}");
        return query.Count == 0 ? path : $"{path}?{string.Join('&', query)}";
    }

    // ---- Request building -----------------------------------------------

    // The API accepts at most 50 MB per PDF / request.
    private const long MaxFileBytes = 50L * 1024 * 1024;

    private static void ValidateLetterRequest(LetterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.File);
        ArgumentNullException.ThrowIfNull(request.Specification);

        if (request.File.Length == 0)
        {
            throw new ArgumentException("The letter file must not be empty.", nameof(request));
        }

        // The 50 MB limit applies per request, so count the main file plus all secondary PDFs.
        long totalBytes = request.File.LongLength;
        if (request.Attachments is { } attachments)
        {
            totalBytes += attachments.Where(a => a is not null).Sum(a => a.LongLength);
        }
        if (request.Backgrounds is { } bg)
        {
            totalBytes += (bg.FirstPage?.LongLength ?? 0) + (bg.OtherPages?.LongLength ?? 0);
        }
        if (request.TermsAndConditions?.Terms is { } terms)
        {
            totalBytes += terms.LongLength;
        }

        if (totalBytes > MaxFileBytes)
        {
            throw new ArgumentException(
                $"The request exceeds the {MaxFileBytes / (1024 * 1024)} MB limit (PDFs total {totalBytes} bytes).",
                nameof(request));
        }

        // Per the spec, a serial letter cannot be combined with an email_letter object.
        if (request.SerialLetter is not null && request.EmailLetter is not null)
        {
            throw new ArgumentException(
                "SerialLetter and EmailLetter cannot be combined in a single request.", nameof(request));
        }

        // A numeric serial separator must actually be a positive whole number, otherwise the wire
        // shape (pages_separator_type = "number") would carry a string value, violating the spec.
        if (request.SerialLetter is { PagesSeparatorType: SeparatorType.Number } serial
            && (!long.TryParse(serial.PagesSeparatorValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pages) || pages <= 0))
        {
            throw new ArgumentException(
                "SerialLetter.PagesSeparatorValue must be a positive whole number when PagesSeparatorType is Number.",
                nameof(request));
        }

        // Registered mail can only be sent nationally.
        if (request.Registered != RegisteredMail.None && request.Specification.Shipping == ShippingType.International)
        {
            throw new ArgumentException(
                "Registered mail can only be sent nationally.", nameof(request));
        }

        // The dispatch date, if set, must not be in the past (the server enforces "in the future").
        if (request.DispatchDate is { } dispatch && dispatch < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("DispatchDate must not be in the past.", nameof(request));
        }
    }

    private static LxLetterWire BuildLetterWire(LetterRequest request)
    {
        var (base64File, checksum) = EncodeFile(request.File);
        var spec = request.Specification;

        var wire = new LxLetterWire
        {
            Base64File = base64File,
            Base64FileChecksum = checksum,
            Specification = new LxSpecificationWire
            {
                Color = ColorToWire(spec.Color),
                Mode = ModeToWire(spec.Mode),
                Shipping = ShippingToWire(spec.Shipping),
                C4 = spec.C4 ? 1 : 0
            },
            FilenameOriginal = request.FilenameOriginal,
            Registered = RegisteredToWire(request.Registered),
            DispatchDate = request.DispatchDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Notice = request.Notice
        };

        if (request.SerialLetter is { } sl)
        {
            // For separator type "number" the API expects a JSON number, otherwise a JSON string (keyword).
            JsonNode separatorValue =
                sl.PagesSeparatorType == SeparatorType.Number
                && long.TryParse(sl.PagesSeparatorValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pageCount)
                    ? JsonValue.Create(pageCount)
                    : JsonValue.Create(sl.PagesSeparatorValue);

            wire.SerialLetter = new LxSerialWire
            {
                PagesSeparatorType = SeparatorTypeToWire(sl.PagesSeparatorType),
                PagesSeparatorValue = separatorValue
            };
        }

        if (request.EmailLetter is { } el)
        {
            wire.EmailLetter = new LxEmailLetterWire
            {
                EmailOption = EmailOptionToWire(el.EmailOption),
                EmailReceiver = el.EmailReceiver
            };
        }

        if (request.Attachments is { Count: > 0 } attachments)
        {
            wire.Base64Attachments = attachments.Select(Convert.ToBase64String).ToList();
        }

        if (request.Backgrounds is { } bg)
        {
            wire.Backgrounds = new LxBackgroundsWire
            {
                Base64BackgroundFirstPage = bg.FirstPage is null ? null : Convert.ToBase64String(bg.FirstPage),
                Base64BackgroundOtherPages = bg.OtherPages is null ? null : Convert.ToBase64String(bg.OtherPages)
            };
        }

        if (request.TermsAndConditions is { } tc)
        {
            wire.TermsAndConditions = new LxTermsWire
            {
                Base64Terms = Convert.ToBase64String(tc.Terms),
                TermsOnAllPages = tc.OnAllPages ? 1 : 0
            };
        }

        if (request.BankForm is { } bf)
        {
            wire.BankForm = new LxBankFormWire
            {
                BankFormIncluded = bf.Included ? 1 : 0,
                Payee = bf.Payee,
                Iban = bf.Iban,
                Bic = bf.Bic,
                Amount = bf.Amount,
                PurposeOfPayment = bf.PurposeOfPayment,
                PurposeOfPayment2 = bf.PurposeOfPayment2
            };
        }

        return wire;
    }

    internal static (string Base64File, string Checksum) EncodeFile(byte[] file)
    {
        var base64File = Convert.ToBase64String(file);
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(base64File));
        var checksum = Convert.ToHexString(hash).ToLowerInvariant();
        return (base64File, checksum);
    }

    // ---- Response mapping -----------------------------------------------

    private EmailJobResult ParseEmailCreate(JsonElement data)
    {
        if (data.ValueKind == JsonValueKind.Array)
        {
            return new EmailJobResult { EmailJobs = DeserializeEmailJobs(data) };
        }

        if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("emailjobs", out _))
        {
            var combined = data.Deserialize(LetterXpressJsonContext.Default.LxEmailCreateData);
            return new EmailJobResult
            {
                EmailJobs = combined?.EmailJobs?.Select(MapEmailJob).ToList() ?? [],
                PrintJobs = combined?.PrintJobs?.Select(MapPrintJob).ToList() ?? []
            };
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            var single = data.Deserialize(LetterXpressJsonContext.Default.LxEmailJobWire);
            return new EmailJobResult { EmailJobs = single is null ? [] : [MapEmailJob(single)] };
        }

        return new EmailJobResult();
    }

    private PagedResult<EmailJob> ParseEmailList(JsonElement data)
    {
        if (data.ValueKind == JsonValueKind.Array)
        {
            return new PagedResult<EmailJob> { Items = DeserializeEmailJobs(data) };
        }

        if (data.ValueKind != JsonValueKind.Object)
        {
            return new PagedResult<EmailJob>();
        }

        Pagination? pagination = null;
        if (data.TryGetProperty("pagination", out var paginationElement))
        {
            pagination = MapPagination(paginationElement.Deserialize(LetterXpressJsonContext.Default.LxPaginationWire));
        }

        var jobs = new List<EmailJob>();
        if (data.TryGetProperty("emailjobs", out var emailJobsElement) && emailJobsElement.ValueKind == JsonValueKind.Array)
        {
            jobs = DeserializeEmailJobs(emailJobsElement);
        }
        else if (data.TryGetProperty("id", out _))
        {
            // Defensive: a single e-mail job object without an explicit array wrapper.
            var single = data.Deserialize(LetterXpressJsonContext.Default.LxEmailJobWire);
            if (single is not null) jobs.Add(MapEmailJob(single));
        }

        return new PagedResult<EmailJob> { Items = jobs, Pagination = pagination };
    }

    private static string? RawTextOrNull(JsonElement element) =>
        element.ValueKind == JsonValueKind.Undefined ? null : element.GetRawText();

    private List<EmailJob> DeserializeEmailJobs(JsonElement array)
    {
        var jobs = new List<EmailJob>();
        foreach (var element in array.EnumerateArray())
        {
            var wire = element.Deserialize(LetterXpressJsonContext.Default.LxEmailJobWire);
            if (wire is not null) jobs.Add(MapEmailJob(wire));
        }
        return jobs;
    }

    private static PrintJob MapPrintJob(LxPrintJobWire w) => new()
    {
        Id = w.Id,
        Shipping = w.Shipping,
        Mode = w.Mode,
        Color = w.Color,
        C4 = w.C4 != 0,
        Registered = w.Registered,
        BankForm = w.BankForm != 0,
        Notice = w.Notice,
        Status = w.Status,
        DispatchDate = w.DispatchDate,
        FilenameOriginal = w.FilenameOriginal,
        CreatedAt = ParseDateTime(w.CreatedAt),
        UpdatedAt = ParseDateTime(w.UpdatedAt),
        Items = w.Items?.Select(MapPrintJobItem).ToList() ?? []
    };

    private static PrintJobItem MapPrintJobItem(LxPrintJobItemWire w) => new()
    {
        Address = w.Address,
        Pages = w.Pages,
        Amount = w.Amount,
        Vat = w.Vat,
        Status = w.Status,
        TrackingCode = w.TrackingCode,
        Base64Data = w.Base64Data
    };

    private static EmailJob MapEmailJob(LxEmailJobWire w) => new()
    {
        Id = w.Id,
        EmailSender = w.EmailSender,
        EmailReceiver = w.EmailReceiver,
        EmailOption = w.EmailOption,
        SentAt = ParseDateTime(w.SentAt),
        Amount = w.Amount,
        Vat = w.Vat,
        Status = w.Status,
        Subject = w.Subject,
        Content = w.Content,
        Footer = w.Footer,
        CreatedAt = ParseDateTime(w.CreatedAt),
        PrintJobId = w.PrintjobId,
        PrintJob = w.Printjob is null ? null : MapPrintJob(w.Printjob)
    };

    private static Transaction MapTransaction(LxTransactionWire w) => new()
    {
        Amount = w.Amount,
        Currency = w.Currency,
        Description = w.Description,
        CreatedAt = ParseDateTime(w.CreatedAt)
    };

    private static Invoice MapInvoice(LxInvoiceWire w) => new()
    {
        Id = w.Id,
        Amount = w.Amount,
        Vat = w.Vat,
        InvoiceDate = ParseDate(w.InvoiceDate),
        Base64Data = w.Base64Data
    };

    private static Pagination? MapPagination(LxPaginationWire? w) => w is null ? null : new Pagination
    {
        Total = w.Total,
        Count = w.Count,
        CurrentPage = w.CurrentPage,
        LastPage = w.LastPage,
        PerPage = w.PerPage,
        FirstPageUrl = w.FirstPageUrl,
        LastPageUrl = w.LastPageUrl,
        NextPageUrl = w.NextPageUrl,
        PrevPageUrl = w.PrevPageUrl
    };

    // LetterXpress returns timestamps without a timezone designator, in German local time
    // (Europe/Berlin, i.e. CET/CEST). Verified against the live API.
    private static readonly TimeZoneInfo? BerlinTimeZone = ResolveBerlinTimeZone();

    private static TimeZoneInfo? ResolveBerlinTimeZone()
    {
        foreach (var id in (string[])["Europe/Berlin", "W. Europe Standard Time"])
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // Try the next identifier.
            }
            catch (InvalidTimeZoneException)
            {
                // Try the next identifier.
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseDateTime(string? value)
    {
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return null;
        }

        var unspecified = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
        var offset = BerlinTimeZone?.GetUtcOffset(unspecified) ?? TimeSpan.Zero;
        return new DateTimeOffset(unspecified, offset);
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;

    // ---- Enum mapping ---------------------------------------------------

    private static string ColorToWire(LetterColor color) => color switch
    {
        LetterColor.BlackWhite => "1",
        LetterColor.Color => "4",
        _ => "1"
    };

    private static string ModeToWire(PrintMode mode) => mode switch
    {
        PrintMode.Simplex => "simplex",
        PrintMode.Duplex => "duplex",
        _ => "simplex"
    };

    private static string ShippingToWire(ShippingType shipping) => shipping switch
    {
        ShippingType.National => "national",
        ShippingType.International => "international",
        ShippingType.Auto => "auto",
        _ => "auto"
    };

    private static string? RegisteredToWire(RegisteredMail registered) => registered switch
    {
        RegisteredMail.EinschreibenEinwurf => "r1",
        RegisteredMail.Einschreiben => "r2",
        _ => null
    };

    private static string EmailOptionToWire(EmailOption option) => option switch
    {
        EmailOption.MailDirect => "maildirect",
        EmailOption.MailPlus => "mailplus",
        EmailOption.MailSecure => "mailsecure",
        _ => "maildirect"
    };

    private static string SeparatorTypeToWire(SeparatorType type) => type switch
    {
        SeparatorType.String => "string",
        SeparatorType.Number => "number",
        _ => "string"
    };

    private static string PrintJobFilterToWire(PrintJobFilter filter) => filter switch
    {
        PrintJobFilter.Queue => "queue",
        PrintJobFilter.Hold => "hold",
        PrintJobFilter.Done => "done",
        PrintJobFilter.Canceled => "canceled",
        PrintJobFilter.Draft => "draft",
        _ => "queue"
    };

    private static string EmailJobFilterToWire(EmailJobFilter filter) => filter switch
    {
        EmailJobFilter.Queue => "queue",
        EmailJobFilter.Hold => "hold",
        EmailJobFilter.Canceled => "canceled",
        EmailJobFilter.Draft => "draft",
        EmailJobFilter.Success => "success",
        _ => "queue"
    };

    private static string TransactionFilterToWire(TransactionFilter filter) => filter switch
    {
        TransactionFilter.PayIns => "payins",
        TransactionFilter.PayOuts => "payouts",
        TransactionFilter.PrintJobs => "printjobs",
        _ => "payins"
    };
}
