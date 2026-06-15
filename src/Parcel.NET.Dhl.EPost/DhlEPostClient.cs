using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.EPost.Internal;
using Parcel.NET.Dhl.EPost.Models;

namespace Parcel.NET.Dhl.EPost;

/// <summary>Deutsche Post E-POSTBUSINESS API v2 client implementing <see cref="IDhlEPostClient"/>.</summary>
public class DhlEPostClient : IDhlEPostClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Initializes a new instance of <see cref="DhlEPostClient"/>.</summary>
    /// <param name="httpClient">The configured HTTP client for E-POST API requests (auth handler attached).</param>
    public DhlEPostClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EPostLetterResult>> SubmitLettersAsync(
        IEnumerable<EPostLetterRequest> letters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(letters);

        var letterList = letters as IReadOnlyList<EPostLetterRequest> ?? letters.ToArray();
        if (letterList.Count == 0)
        {
            return [];
        }

        // Request-level preflight (cheap, before any base64 encoding): reject if the total exceeds 300 MB.
        long totalBytes = 0;
        foreach (var letter in letterList)
        {
            ArgumentNullException.ThrowIfNull(letter);
            ArgumentNullException.ThrowIfNull(letter.PdfContent);
            totalBytes += letter.PdfContent.LongLength;
        }

        if (totalBytes > MaxRequestBytes)
        {
            throw new ArgumentException(
                $"Total PDF size is {totalBytes} bytes; the E-POST per-request limit is {MaxRequestBytes} bytes (300 MB). Split into multiple requests.",
                nameof(letters));
        }

        // Map each letter (validates required fields + the 20 MB per-shipment limit, encodes to base64).
        var payload = new DhlEPostLetter[letterList.Count];
        for (var i = 0; i < letterList.Count; i++)
        {
            payload[i] = MapToApi(letterList[i]);
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "api/Letter",
            payload,
            DhlEPostJsonContext.Default.DhlEPostLetterArray,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var idents = await response.Content.ReadFromJsonAsync(
            DhlEPostJsonContext.Default.DhlEPostLetterIdentArray,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize E-POST letter submission response.");

        return idents.Select(i => new EPostLetterResult
        {
            FileName = i.FileName,
            LetterId = i.LetterID
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task<EPostLetterStatus> GetLetterStatusAsync(long letterId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/Letter/{letterId}",
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var status = await response.Content.ReadFromJsonAsync(
            DhlEPostJsonContext.Default.DhlEPostLetterStatus,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize E-POST letter status response.");

        return MapStatus(status);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EPostLetterStatus>> GetLetterStatusesAsync(
        IEnumerable<long> letterIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(letterIds);

        var ids = letterIds.ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "api/Letter/StatusQuery",
            ids,
            DhlEPostJsonContext.Default.Int64Array,
            cancellationToken).ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var statuses = await response.Content.ReadFromJsonAsync(
            DhlEPostJsonContext.Default.DhlEPostLetterStatusArray,
            cancellationToken).ConfigureAwait(false)
            ?? throw new ParcelException("Failed to deserialize E-POST status query response.");

        return statuses.Select(MapStatus).ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "api/Login/HealthCheck",
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        // A 200 still carries an Error object: I501 = OK, W501 = maintenance announced (still up), E501 = inactive.
        // An empty/unparseable body on a 2xx is treated as healthy.
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var status = DhlEPostErrorParser.TryParse(rawBody);

        return status is null
            || !string.Equals(status.Level, "Error", StringComparison.OrdinalIgnoreCase);
    }

    // The API caps the original PDF at 20 MB per shipment and 300 MB per request (see the spec).
    private const int MaxPdfBytes = 20 * 1024 * 1024;
    private const long MaxRequestBytes = 300L * 1024 * 1024;

    private static DhlEPostLetter MapToApi(EPostLetterRequest r)
    {
        ArgumentNullException.ThrowIfNull(r);
        ArgumentNullException.ThrowIfNull(r.PdfContent);

        // The API requires these fields; null/blank would be silently dropped (WhenWritingNull), so fail fast instead.
        ArgumentException.ThrowIfNullOrWhiteSpace(r.FileName, nameof(r.FileName));
        ArgumentException.ThrowIfNullOrWhiteSpace(r.AddressLine1, nameof(r.AddressLine1));
        ArgumentException.ThrowIfNullOrWhiteSpace(r.ZipCode, nameof(r.ZipCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(r.City, nameof(r.City));

        if (r.PdfContent.Length == 0)
        {
            throw new ArgumentException($"PDF content for '{r.FileName}' is empty.", nameof(r.PdfContent));
        }

        if (r.PdfContent.Length > MaxPdfBytes)
        {
            throw new ArgumentException(
                $"PDF content for '{r.FileName}' is {r.PdfContent.Length} bytes; the E-POST limit is {MaxPdfBytes} bytes (20 MB).",
                nameof(r.PdfContent));
        }

        return new DhlEPostLetter
        {
            FileName = r.FileName,
            Data = Convert.ToBase64String(r.PdfContent),
            AddressLine1 = r.AddressLine1,
            AddressLine2 = r.AddressLine2,
            AddressLine3 = r.AddressLine3,
            ZipCode = r.ZipCode,
            City = r.City,
            Country = r.Country,
            SenderAdressLine1 = r.SenderLine1,
            SenderStreet = r.SenderStreet,
            SenderZipCode = r.SenderZipCode,
            SenderCity = r.SenderCity,
            IsColor = r.Color ? true : null,
            IsDuplex = r.Duplex ? true : null,
            RegisteredLetter = r.RegisteredLetter.ToApiValue(),
            ActivateDuplicateFailsafe = r.ActivateDuplicateFailsafe ? true : null,
            TestFlag = r.Test ? true : null,
            TestEMail = r.TestEMail,
            BatchID = r.BatchId,
            Custom1 = r.Custom1
        };
    }

    private static EPostLetterStatus MapStatus(DhlEPostLetterStatus s) => new()
    {
        LetterId = s.LetterID,
        FileName = s.FileName,
        RawStatusId = s.StatusID,
        Stage = Enum.IsDefined(typeof(EPostLetterStage), s.StatusID)
            ? (EPostLetterStage)s.StatusID
            : EPostLetterStage.Unknown,
        PageCount = s.NoOfPages,
        RegisteredLetterId = s.RegisteredLetterID,
        Custom1 = s.Custom1,
        Messages = s.ErrorList?.Select(e => new EPostMessage
        {
            Level = e.Level,
            Code = e.Code,
            Description = e.Description
        }).ToArray() ?? []
    };

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Error responses carry an Error object ({level, code, description}); surface code + description when present.
        var apiError = DhlEPostErrorParser.TryParse(rawBody);

        // 429 = rate limit exceeded (e.g. status queries must keep a >=5s gap).
        var message = response.StatusCode == HttpStatusCode.TooManyRequests
            ? "E-POST API rate limit exceeded (HTTP 429). Throttle status queries (>=5s apart) and retry."
            : apiError is not null
                ? $"E-POST API returned {(int)response.StatusCode}: [{apiError.Code}] {apiError.Description}"
                : $"E-POST API returned {(int)response.StatusCode}.";

        throw new ParcelException(
            message,
            response.StatusCode,
            apiError?.Code ?? ((int)response.StatusCode).ToString(),
            rawBody);
    }
}
