namespace Parcel.NET.Dhl.Shipping.Models;

/// <summary>
/// DHL-specific consignee delivery options.
/// </summary>
public class DhlConsignee
{
    /// <summary>
    /// Gets the type of delivery address. Defaults to <see cref="DhlConsigneeType.ContactAddress"/>.
    /// </summary>
    public DhlConsigneeType Type { get; init; } = DhlConsigneeType.ContactAddress;

    /// <summary>
    /// Gets the Packstation locker ID (100-999), if delivering to a locker.
    /// </summary>
    public int? LockerId { get; init; }

    /// <summary>
    /// Gets the recipient's DHL post number (6-10 digits), required for Packstation delivery.
    /// </summary>
    public string? PostNumber { get; init; }

    /// <summary>
    /// Gets the retail ID (post office/Filiale number), if delivering to a post office.
    /// </summary>
    public int? RetailId { get; init; }

    /// <summary>
    /// Gets the PO box ID, if delivering to a PO box.
    /// </summary>
    public int? PoBoxId { get; init; }
}

/// <summary>
/// Type of DHL consignee delivery address.
/// </summary>
public enum DhlConsigneeType
{
    /// <summary>Standard contact address.</summary>
    ContactAddress,
    /// <summary>DHL Packstation locker.</summary>
    Locker,
    /// <summary>DHL post office (Filiale).</summary>
    PostOffice,
    /// <summary>PO Box (Postfach).</summary>
    POBox
}
