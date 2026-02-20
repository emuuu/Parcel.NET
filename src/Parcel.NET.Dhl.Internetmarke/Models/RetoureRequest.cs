namespace Parcel.NET.Dhl.Internetmarke.Models;

/// <summary>
/// Request for a refund/retoure of purchased vouchers (POST /app/retoure).
/// </summary>
public class RetoureRequest
{
    /// <summary>
    /// Gets or sets an optional shop-side retoure identifier.
    /// </summary>
    public string? ShopRetoureId { get; set; }

    /// <summary>
    /// Gets or sets the voucher IDs to request refund for.
    /// </summary>
    public required List<string> VoucherIds { get; set; }
}

/// <summary>
/// Result of a refund/retoure request (POST /app/retoure).
/// </summary>
public class RetoureResult
{
    /// <summary>
    /// Gets or sets the retoure ID assigned by DHL.
    /// </summary>
    public string? RetoureId { get; set; }

    /// <summary>
    /// Gets or sets the shop retoure ID.
    /// </summary>
    public string? ShopRetoureId { get; set; }
}

/// <summary>
/// State of a retoure/refund request (GET /app/retoure).
/// </summary>
public class RetoureState
{
    /// <summary>
    /// Gets or sets the retoure ID.
    /// </summary>
    public string? RetoureId { get; set; }

    /// <summary>
    /// Gets or sets the shop retoure ID.
    /// </summary>
    public string? ShopRetoureId { get; set; }

    /// <summary>
    /// Gets or sets the current state of the retoure.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the retoure transaction ID.
    /// </summary>
    public string? RetoureTransactionId { get; set; }
}
