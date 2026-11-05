using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Parcel.NET.LetterXpress.Letters.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.LetterXpress.Letters.Tests;

// xunit.v3 runs test classes in parallel; allocation measurements need the process to themselves.
[Collection("Allocation measurements")]
public class LetterXpressMemoryTests
{
    /// <summary>Copies the request body into a discarding stream (no buffering) and counts the bytes.</summary>
    private sealed class DrainingHandler : HttpMessageHandler
    {
        public long BodyBytes { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var counting = new CountingNullStream();
            await request.Content!.CopyToAsync(counting, cancellationToken);
            BodyBytes = counting.Length;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(LetterXpressStreamingTests.OkPrintJob) };
        }
    }

    private const int MainFileBytes = 8 * 1024 * 1024;
    private const int AttachmentBytes = 1024 * 1024;

    [Fact]
    public async Task CreatePrintJobAsync_LargeFile_DoesNotBufferThePayloadInMemory()
    {
        var request = new LetterRequest
        {
            File = LetterXpressStreamingTests.RandomBytes(MainFileBytes),
            Attachments = [LetterXpressStreamingTests.RandomBytes(AttachmentBytes, 9)],
            Specification = new LetterSpecification { Shipping = ShippingType.National }
        };
        var handler = new DrainingHandler();
        var client = LetterXpressStreamingTests.CreateClient(handler);

        // Warm up (JIT, caches) with a small payload so only the large request is measured.
        await client.CreatePrintJobAsync(new LetterRequest { File = LetterXpressStreamingTests.RandomBytes(10), Specification = new LetterSpecification() });

        var before = GC.GetTotalAllocatedBytes(precise: true);
        await client.CreatePrintJobAsync(request);
        var allocated = GC.GetTotalAllocatedBytes(precise: true) - before;

        var rawBytes = (long)MainFileBytes + AttachmentBytes;
        handler.BodyBytes.ShouldBeGreaterThan(rawBytes * 4 / 3);
        allocated.ShouldBeLessThanOrEqualTo(3 * rawBytes, $"allocated {allocated:N0} bytes for {rawBytes:N0} bytes of PDFs");
        Console.WriteLine($"[memory] streamed: allocated {allocated:N0} bytes for {rawBytes:N0} raw PDF bytes ({(double)allocated / rawBytes:F2}x)");
    }

    [Fact]
    public async Task LegacyStringBody_AllocatesFarMoreThanTheStreamedBody_Reference()
    {
        // Reference measurement of the 1.0.1 approach (Base64 string + MD5 copy + JsonContent over the string model).
        var file = LetterXpressStreamingTests.RandomBytes(MainFileBytes);
        var attachment = LetterXpressStreamingTests.RandomBytes(AttachmentBytes, 9);
        var legacyPayload = new
        {
            auth = new { username = "u", apikey = "k", mode = "live" },
            letter = new { base64_file = "", base64_file_checksum = "", base64_attachments = new List<string>() }
        };

        // Warm up.
        _ = JsonContent.Create(legacyPayload, options: LetterXpressStreamingTests.LegacyOptions).Headers;

        var before = GC.GetTotalAllocatedBytes(precise: true);
        var base64 = Convert.ToBase64String(file);
        var checksum = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(base64))).ToLowerInvariant();
        var payload = new
        {
            auth = legacyPayload.auth,
            letter = new { base64_file = base64, base64_file_checksum = checksum, base64_attachments = new List<string> { Convert.ToBase64String(attachment) } }
        };
        var content = JsonContent.Create(payload, options: LetterXpressStreamingTests.LegacyOptions);
        await content.CopyToAsync(new CountingNullStream());
        var allocated = GC.GetTotalAllocatedBytes(precise: true) - before;

        var rawBytes = (long)MainFileBytes + AttachmentBytes;
        Console.WriteLine($"[memory] legacy:   allocated {allocated:N0} bytes for {rawBytes:N0} raw PDF bytes ({(double)allocated / rawBytes:F2}x)");
        allocated.ShouldBeGreaterThan(3 * rawBytes);
    }
}

[CollectionDefinition("Allocation measurements", DisableParallelization = true)]
public sealed class AllocationMeasurementsCollection;
