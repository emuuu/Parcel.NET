namespace Parcel.NET.Dhl.EPost.Models;

/// <summary>Result of submitting a single letter — maps the source file name to the assigned shipment id.</summary>
public class EPostLetterResult
{
    /// <summary>The file name supplied in the request.</summary>
    public required string FileName { get; set; }

    /// <summary>The unique shipment id assigned by E-POST, used for later status queries.</summary>
    public long LetterId { get; set; }
}
