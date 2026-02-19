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
    /// Gets the Packstation locker ID, if delivering to a locker.
    /// </summary>
    public string? LockerId { get; init; }

    /// <summary>
    /// Gets the post office ID, if delivering to a post office.
    /// </summary>
    public string? PostOfficeId { get; init; }

    /// <summary>
    /// Gets the PO box ID, if delivering to a PO box.
    /// </summary>
    public string? PoBoxId { get; init; }
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
