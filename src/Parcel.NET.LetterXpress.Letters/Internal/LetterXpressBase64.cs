using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Parcel.NET.LetterXpress.Letters.Internal;

internal static class LetterXpressBase64
{
    // Input block size; must be a multiple of 3 so that only the final block is padded.
    private const int InputBlockSize = 3 * 8192;

    /// <summary>
    /// Computes the MD5 checksum (lower-case hex) over the ASCII bytes of the padded Base64 representation of
    /// <paramref name="data"/>, encoding block by block so the full Base64 text is never materialized.
    /// </summary>
    internal static string ComputeChecksum(ReadOnlySpan<byte> data) =>
        ComputeChecksum(data, InputBlockSize);

    internal static string ComputeChecksum(ReadOnlySpan<byte> data, int inputBlockSize)
    {
        if (inputBlockSize <= 0 || inputBlockSize % 3 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputBlockSize), "The block size must be a positive multiple of 3.");
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        var buffer = ArrayPool<byte>.Shared.Rent(Base64.GetMaxEncodedToUtf8Length(inputBlockSize));
        try
        {
            while (!data.IsEmpty)
            {
                var block = data.Length > inputBlockSize ? data[..inputBlockSize] : data;
                var status = Base64.EncodeToUtf8(block, buffer, out var consumed, out var written, isFinalBlock: block.Length == data.Length);
                if (status != OperationStatus.Done || consumed != block.Length)
                {
                    throw new InvalidOperationException("Base64 encoding of a checksum block did not complete.");
                }

                hash.AppendData(buffer, 0, written);
                data = data[consumed..];
            }

            return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
