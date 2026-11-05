using System.Net;
using System.Text.Json;

namespace Parcel.NET.LetterXpress.Letters.Internal;

/// <summary>
/// Request body for print and e-mail jobs. The JSON is written straight into the request stream when the request
/// is sent (PDFs as Base64 via <see cref="Utf8JsonWriter"/>, without an intermediate Base64 string), and the exact
/// length is reported so the request carries a <c>Content-Length</c> header instead of using chunked transfer encoding.
/// The content can be serialized any number of times.
/// </summary>
internal sealed class LetterXpressLetterContent : HttpContent
{
    private readonly LxLetterRequest _payload;
    private long? _length;

    internal LetterXpressLetterContent(LxLetterRequest payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        _payload = payload;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
        JsonSerializer.SerializeAsync(stream, _payload, LetterXpressJsonContext.Default.LxLetterRequest, cancellationToken);

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        JsonSerializer.Serialize(stream, _payload, LetterXpressJsonContext.Default.LxLetterRequest);
    }

    protected override bool TryComputeLength(out long length)
    {
        // Serialize a copy without the PDF payloads (small) and add the Base64 characters of the payloads, which are
        // plain ASCII and never escaped: exact without a second full serialization pass.
        _length ??= ComputeLength();
        length = _length.Value;
        return true;
    }

    private long ComputeLength()
    {
        var skeleton = new LxLetterRequest { Auth = _payload.Auth, Letter = _payload.Letter.CopyWithoutPayloads(out var base64Length) };
        return JsonSerializer.SerializeToUtf8Bytes(skeleton, LetterXpressJsonContext.Default.LxLetterRequest).LongLength + base64Length;
    }
}
