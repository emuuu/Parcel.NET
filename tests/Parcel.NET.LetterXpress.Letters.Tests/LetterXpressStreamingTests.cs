using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Parcel.NET.LetterXpress.Letters.Internal;
using Parcel.NET.LetterXpress.Letters.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.LetterXpress.Letters.Tests;

public class LetterXpressStreamingTests
{
    internal const string OkPrintJob = """{"status":200,"message":"OK","data":{"id":1,"status":"queue"}}""";
    private const string OkEmailJob = """{"status":200,"message":"OK","data":{"id":2,"status":"queue"}}""";

    internal static byte[] RandomBytes(int length, int seed = 42)
    {
        var bytes = new byte[length];
        new Random(seed).NextBytes(bytes);
        return bytes;
    }

    internal static LetterXpressClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.letterxpress.de/v3/") };
        var options = Options.Create(new LetterXpressOptions { Username = "u", ApiKey = "k" });
        return new LetterXpressClient(httpClient, options);
    }

    // ---- Checksum -------------------------------------------------------

    private static string LegacyChecksum(byte[] file) =>
        Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(Convert.ToBase64String(file)))).ToLowerInvariant();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(299)] // n % 3 == 2
    [InlineData(300)] // n % 3 == 0
    [InlineData(301)] // n % 3 == 1
    [InlineData(24575)] // around the default block size (24576)
    [InlineData(24576)]
    [InlineData(24577)]
    [InlineData(24578)]
    [InlineData(49151)]
    [InlineData(49152)]
    [InlineData(49153)]
    [InlineData(100_001)]
    public void ComputeChecksum_MatchesLegacyAlgorithm(int length)
    {
        var file = RandomBytes(length);

        LetterXpressBase64.ComputeChecksum(file).ShouldBe(LegacyChecksum(file));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(30)]
    public void ComputeChecksum_SmallBlocks_CrossBlockBoundaries(int blockSize)
    {
        for (var length = 0; length <= 100; length++)
        {
            var file = RandomBytes(length, seed: length);

            LetterXpressBase64.ComputeChecksum(file, blockSize).ShouldBe(LegacyChecksum(file), $"length {length}, block {blockSize}");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(4)]
    public void ComputeChecksum_BlockSizeNotMultipleOfThree_Throws(int blockSize) =>
        Should.Throw<ArgumentOutOfRangeException>(() => LetterXpressBase64.ComputeChecksum([1, 2, 3], blockSize));

    // ---- Wire content: old (string) vs. new (streamed) ------------------

    // Replica of the 1.0.1 string-based wire model, serialized the old way (JsonContent, Base64 strings).
    internal static readonly JsonSerializerOptions LegacyOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static string B64(byte[]? bytes) => bytes is null ? null! : Convert.ToBase64String(bytes);

    private static JsonNode LegacyBody(LetterRequest request, string registered, bool withSerial, bool withEmail)
    {
        var spec = request.Specification;
        var legacy = new
        {
            auth = new { username = "u", apikey = "k", mode = "live" },
            letter = new
            {
                base64_file = B64(request.File),
                base64_file_checksum = LegacyChecksum(request.File),
                specification = new
                {
                    color = spec.Color == LetterColor.Color ? "4" : "1",
                    mode = spec.Mode == PrintMode.Duplex ? "duplex" : "simplex",
                    shipping = spec.Shipping == ShippingType.National ? "national" : "auto",
                    c4 = spec.C4 ? 1 : 0
                },
                filename_original = request.FilenameOriginal,
                registered = registered,
                dispatch_date = request.DispatchDate?.ToString("yyyy-MM-dd"),
                serial_letter = withSerial
                    ? new { pages_separator_type = "string", pages_separator_value = (object)request.SerialLetter!.PagesSeparatorValue }
                    : null,
                email_letter = withEmail
                    ? new { email_option = "mailplus", email_receiver = request.EmailLetter!.EmailReceiver }
                    : null,
                base64_attachments = request.Attachments?.Select(B64).ToList(),
                backgrounds = request.Backgrounds is { } bg
                    ? new { base64_background_first_page = B64(bg.FirstPage), base64_background_other_pages = B64(bg.OtherPages) }
                    : null,
                terms_and_conditions = request.TermsAndConditions is { } tc
                    ? new { base64_terms = B64(tc.Terms), terms_on_all_pages = tc.OnAllPages ? 1 : 0 }
                    : null,
                bank_form = request.BankForm is { } bf
                    ? new
                    {
                        bank_form_included = bf.Included ? 1 : 0,
                        payee = bf.Payee,
                        iban = bf.Iban,
                        bic = bf.Bic,
                        amount = bf.Amount,
                        purpose_of_payment = bf.PurposeOfPayment,
                        purpose_of_payment2 = bf.PurposeOfPayment2
                    }
                    : null,
                notice = request.Notice
            }
        };

        // The old code path: JsonContent.Create(payload, typeInfo) -> body string.
        var content = JsonContent.Create(legacy, options: LegacyOptions);
        return JsonNode.Parse(content.ReadAsStringAsync().GetAwaiter().GetResult())!;
    }

    private static LetterRequest FullRequest(bool serial, bool email)
    {
        var request = new LetterRequest
        {
            File = RandomBytes(10_001, 1),
            Specification = new LetterSpecification { Color = LetterColor.Color, Mode = PrintMode.Duplex, Shipping = ShippingType.National, C4 = true },
            FilenameOriginal = "Rechnung-Größe-日本語.pdf",
            Registered = RegisteredMail.Einschreiben,
            DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3),
            Attachments = [RandomBytes(1, 2), RandomBytes(2, 3), RandomBytes(3, 4), []],
            Backgrounds = new LetterBackgrounds { FirstPage = RandomBytes(100, 5), OtherPages = RandomBytes(101, 6) },
            TermsAndConditions = new TermsAndConditions { Terms = RandomBytes(102, 7), OnAllPages = true },
            BankForm = new BankForm { Included = true, Payee = "Max Müller", Iban = "DE89370400440532013000", Bic = "COBADEFFXXX", Amount = "12,34", PurposeOfPayment = "Rechnung 1", PurposeOfPayment2 = "Zeile 2" },
            Notice = "Bemerkung mit + und / und \"Anführungszeichen\" € 😀"
        };

        if (serial)
        {
            request.SerialLetter = new SerialLetterOptions { PagesSeparatorType = SeparatorType.String, PagesSeparatorValue = "%%SEPARATOR%%" };
        }

        if (email)
        {
            request.EmailLetter = new EmailLetterOptions { EmailOption = EmailOption.MailPlus, EmailReceiver = "empfaenger@example.org" };
        }

        return request;
    }

    [Theory]
    [InlineData(true, false)] // serial letter
    [InlineData(false, true)] // e-mail letter
    [InlineData(false, false)]
    public async Task CreatePrintJobAsync_FullRequest_BodySemanticallyEqualToLegacyBody(bool serial, bool email)
    {
        var handler = new MockHttpMessageHandler(OkPrintJob);
        var request = FullRequest(serial, email);

        await CreateClient(handler).CreatePrintJobAsync(request);

        var expected = LegacyBody(request, "r2", serial, email);
        JsonNode.DeepEquals(JsonNode.Parse(handler.LastRequestBody!), expected).ShouldBeTrue(
            $"actual: {handler.LastRequestBody}\nexpected: {expected.ToJsonString()}");
    }

    [Fact]
    public async Task CreateEmailJobAsync_FullRequest_BodySemanticallyEqualToLegacyBody()
    {
        var handler = new MockHttpMessageHandler(OkEmailJob);
        var request = FullRequest(serial: false, email: true);

        await CreateClient(handler).CreateEmailJobAsync(request);

        var expected = LegacyBody(request, "r2", withSerial: false, withEmail: true);
        JsonNode.DeepEquals(JsonNode.Parse(handler.LastRequestBody!), expected).ShouldBeTrue(
            $"actual: {handler.LastRequestBody}\nexpected: {expected.ToJsonString()}");
    }

    [Fact]
    public async Task CreatePrintJobAsync_MinimalRequest_BodySemanticallyEqualToLegacyBody()
    {
        var handler = new MockHttpMessageHandler(OkPrintJob);
        var request = new LetterRequest { File = RandomBytes(7), Specification = new LetterSpecification() };

        await CreateClient(handler).CreatePrintJobAsync(request);

        var actual = JsonNode.Parse(handler.LastRequestBody!)!;
        var expected = LegacyBody(request, registered: null!, withSerial: false, withEmail: false);
        JsonNode.DeepEquals(actual, expected).ShouldBeTrue($"actual: {actual.ToJsonString()}\nexpected: {expected.ToJsonString()}");

        // Optional parts are omitted, not sent as null.
        var letter = actual["letter"]!.AsObject();
        foreach (var name in new[] { "filename_original", "registered", "dispatch_date", "serial_letter", "email_letter", "base64_attachments", "backgrounds", "terms_and_conditions", "bank_form", "notice" })
        {
            letter.ContainsKey(name).ShouldBeFalse(name);
        }
    }

    [Fact]
    public async Task CreatePrintJobAsync_SmallPayloadParts_AreSentUnderTheirOwnFields()
    {
        var handler = new MockHttpMessageHandler(OkPrintJob);
        var request = new LetterRequest
        {
            File = [1, 2, 3, 4],
            Specification = new LetterSpecification(),
            Attachments = [[10], [11, 12]],
            Backgrounds = new LetterBackgrounds { FirstPage = [20], OtherPages = [21, 22] },
            TermsAndConditions = new TermsAndConditions { Terms = [30, 31, 32] }
        };

        await CreateClient(handler).CreatePrintJobAsync(request);

        var letter = JsonNode.Parse(handler.LastRequestBody!)!["letter"]!;
        letter["base64_file"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([1, 2, 3, 4]));
        letter["base64_file_checksum"]!.GetValue<string>().ShouldBe(LegacyChecksum([1, 2, 3, 4]));
        letter["base64_attachments"]![0]!.GetValue<string>().ShouldBe(Convert.ToBase64String([10]));
        letter["base64_attachments"]![1]!.GetValue<string>().ShouldBe(Convert.ToBase64String([11, 12]));
        letter["backgrounds"]!["base64_background_first_page"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([20]));
        letter["backgrounds"]!["base64_background_other_pages"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([21, 22]));
        letter["terms_and_conditions"]!["base64_terms"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([30, 31, 32]));
    }

    [Fact]
    public async Task CreatePrintJobAsync_EmptyAttachmentsList_OmitsAttachments()
    {
        var handler = new MockHttpMessageHandler(OkPrintJob);
        var request = new LetterRequest { File = RandomBytes(7), Specification = new LetterSpecification(), Attachments = [] };

        await CreateClient(handler).CreatePrintJobAsync(request);

        JsonNode.Parse(handler.LastRequestBody!)!["letter"]!.AsObject().ContainsKey("base64_attachments").ShouldBeFalse();
    }

    [Fact]
    public async Task CreatePrintJobAsync_NullAttachmentEntry_Throws()
    {
        var handler = new MockHttpMessageHandler(OkPrintJob);
        var request = new LetterRequest { File = RandomBytes(7), Specification = new LetterSpecification(), Attachments = [null!] };

        await Should.ThrowAsync<ArgumentNullException>(() => CreateClient(handler).CreatePrintJobAsync(request));
        handler.LastRequest.ShouldBeNull();
    }

    // ---- Content-Type, Content-Length, repeatability ---------------------

    /// <summary>Records the request headers and the body bytes without buffering the content up front.</summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? ContentType { get; private set; }
        public long? ContentLengthHeader { get; private set; }
        public bool Chunked { get; private set; }
        public byte[] Body { get; private set; } = [];
        public byte[] SecondBody { get; private set; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = request.Content!;
            ContentType = content.Headers.ContentType?.ToString();
            ContentLengthHeader = content.Headers.ContentLength;
            Chunked = request.Headers.TransferEncodingChunked == true;

            // Serialize twice: the content must be repeatable.
            using (var first = new MemoryStream())
            {
                await content.CopyToAsync(first, cancellationToken);
                Body = first.ToArray();
            }

            using (var second = new MemoryStream())
            {
                await content.CopyToAsync(second, cancellationToken);
                SecondBody = second.ToArray();
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OkPrintJob) };
        }
    }

    [Fact]
    public async Task CreatePrintJobAsync_ReportsExactContentLengthAndPlainJsonContentType()
    {
        var handler = new CapturingHandler();
        var request = FullRequest(serial: true, email: false);

        await CreateClient(handler).CreatePrintJobAsync(request);

        handler.ContentType.ShouldBe("application/json");
        handler.Chunked.ShouldBeFalse();
        handler.ContentLengthHeader.ShouldBe(handler.Body.LongLength);
        handler.SecondBody.ShouldBe(handler.Body);
    }

    [Fact]
    public async Task CreateEmailJobAsync_ReportsExactContentLengthAndPlainJsonContentType()
    {
        var handler = new CapturingHandler();
        var request = FullRequest(serial: false, email: true);

        await CreateClient(handler).CreateEmailJobAsync(request);

        handler.ContentType.ShouldBe("application/json");
        handler.ContentLengthHeader.ShouldBe(handler.Body.LongLength);
        handler.SecondBody.ShouldBe(handler.Body);
    }

    public static TheoryData<int> PayloadLengths => new() { 1, 2, 3, 4, 5, 1000, 1001, 1002 };

    [Theory]
    [MemberData(nameof(PayloadLengths))]
    public void Content_Length_MatchesWrittenBytes_ForAllPayloadParts(int length)
    {
        var wire = new LxLetterWire
        {
            Base64File = RandomBytes(length, 1),
            Base64FileChecksum = new string('a', 32),
            Base64Attachments = [RandomBytes(length, 2), []],
            Backgrounds = new LxBackgroundsWire { Base64BackgroundFirstPage = RandomBytes(length, 3), Base64BackgroundOtherPages = null },
            TermsAndConditions = new LxTermsWire { Base64Terms = RandomBytes(length, 4), TermsOnAllPages = 1 },
            Notice = "Größe — 日本語 😀 \"quoted\" + /",
            FilenameOriginal = "ä.pdf"
        };

        AssertLengthMatches(wire);
    }

    [Fact]
    public void Content_Length_MatchesWrittenBytes_ForMinimalPayload() =>
        AssertLengthMatches(new LxLetterWire { Base64File = [1], Base64FileChecksum = new string('0', 32) });

    private static void AssertLengthMatches(LxLetterWire wire)
    {
        var content = new LetterXpressLetterContent(new LxLetterRequest { Letter = wire });
        using var stream = new MemoryStream();

        content.CopyTo(stream, context: null, CancellationToken.None);

        content.Headers.ContentLength.ShouldBe(stream.Length);
        content.Headers.ContentLength.ShouldBe(content.ReadAsByteArrayAsync().GetAwaiter().GetResult().LongLength);
    }

    [Fact]
    public async Task Content_Serialization_HonorsCancellation()
    {
        var content = new LetterXpressLetterContent(new LxLetterRequest
        {
            Letter = new LxLetterWire { Base64File = RandomBytes(1000), Base64FileChecksum = "x" }
        });
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => content.CopyToAsync(Stream.Null, cts.Token));
    }
}

/// <summary>A write-only stream that discards its input and counts the bytes.</summary>
internal sealed class CountingNullStream : Stream
{
    private long _length;
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _length;
    public override long Position { get => _length; set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => _length += count;
    public override void Write(ReadOnlySpan<byte> buffer) => _length += buffer.Length;
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        _length += buffer.Length;
        return ValueTask.CompletedTask;
    }
}
