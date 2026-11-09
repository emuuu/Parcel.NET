using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Dhl.Tracking.Internal;

/// <summary>
/// Maps the current state of a Parcel DE Tracking v0 shipment to a carrier-agnostic <see cref="TrackingStatus"/>.
/// </summary>
/// <remarks>
/// <para>
/// The mapping follows the DHL ICE/RIC/standard event code list ("ICE_Event_RIC_Kombinationen")
/// published with the Parcel DE Tracking API. The inputs are the attributes of the current state:
/// <c>delivery-event-flag</c>, <c>ruecksendung</c>, <c>ice</c> (International Coded Event) and
/// <c>standard-event-code</c> (TTPRO code). The first matching rule wins:
/// </para>
/// <list type="table">
/// <listheader><term>Rule</term><description>Condition, resulting status</description></listheader>
/// <item><term>1</term><description><c>delivery-event-flag="1"</c> or <c>ice="DLVRD"</c> → <see cref="TrackingStatus.Delivered"/></description></item>
/// <item><term>2</term><description><c>ruecksendung="true"</c> or <c>ice="RETRN"</c> → <see cref="TrackingStatus.Returned"/></description></item>
/// <item><term>3</term><description><c>ice</c> is one of <c>DLVRF NTDEL DMGDS DSPSD EXPHD LDEXC MSRTD MVTEX NTPUP CANCL</c>,
/// or <c>standard-event-code</c> is <c>ZN</c>, <c>BV</c> or <c>AN</c> → <see cref="TrackingStatus.Exception"/></description></item>
/// <item><term>4</term><description><c>standard-event-code="ZU"</c> → <see cref="TrackingStatus.Delivered"/></description></item>
/// <item><term>5</term><description><c>standard-event-code="PO"</c> → <see cref="TrackingStatus.OutForDelivery"/></description></item>
/// <item><term>6</term><description><c>standard-event-code</c> is one of <c>ES AE AA EE NB ZO ZF LA GT</c> → <see cref="TrackingStatus.InTransit"/></description></item>
/// <item><term>7</term><description><c>standard-event-code</c> is one of <c>VA DD ST</c> → <see cref="TrackingStatus.PreTransit"/></description></item>
/// <item><term>8</term><description>anything else → <see cref="TrackingStatus.Unknown"/></description></item>
/// </list>
/// <para>
/// <c>ZF</c> (delivery to a branch) and <c>LA</c> (storage) are deliberately reported as <see cref="TrackingStatus.InTransit"/>:
/// the shipment is waiting for pickup but has not been handed to the recipient.
/// Comparisons are ordinal and case-insensitive on trimmed values.
/// </para>
/// </remarks>
internal static class DhlTrackingStatusMapper
{
    private static readonly HashSet<string> ExceptionIceEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "DLVRF", "NTDEL", "DMGDS", "DSPSD", "EXPHD", "LDEXC", "MSRTD", "MVTEX", "NTPUP", "CANCL"
    };

    private static readonly HashSet<string> ExceptionEventCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ZN", "BV", "AN"
    };

    private static readonly HashSet<string> InTransitEventCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ES", "AE", "AA", "EE", "NB", "ZO", "ZF", "LA", "GT"
    };

    private static readonly HashSet<string> PreTransitEventCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "VA", "DD", "ST"
    };

    /// <summary>
    /// Maps the current shipment state to a <see cref="TrackingStatus"/>.
    /// </summary>
    /// <param name="deliveryEventFlag">The <c>delivery-event-flag</c> attribute (<c>1</c> = delivered).</param>
    /// <param name="ruecksendung">The <c>ruecksendung</c> attribute (<c>true</c> = return shipment).</param>
    /// <param name="ice">The <c>ice</c> attribute (International Coded Event).</param>
    /// <param name="standardEventCode">The <c>standard-event-code</c> attribute (TTPRO code).</param>
    internal static TrackingStatus Map(
        string? deliveryEventFlag,
        string? ruecksendung,
        string? ice,
        string? standardEventCode)
    {
        var flag = deliveryEventFlag?.Trim();
        var returned = ruecksendung?.Trim();
        var iceEvent = ice?.Trim();
        var code = standardEventCode?.Trim();

        if (flag == "1" || Is(iceEvent, "DLVRD"))
            return TrackingStatus.Delivered;

        if (Is(returned, "true") || Is(iceEvent, "RETRN"))
            return TrackingStatus.Returned;

        if ((iceEvent is { Length: > 0 } && ExceptionIceEvents.Contains(iceEvent))
            || (code is { Length: > 0 } && ExceptionEventCodes.Contains(code)))
            return TrackingStatus.Exception;

        if (Is(code, "ZU"))
            return TrackingStatus.Delivered;

        if (Is(code, "PO"))
            return TrackingStatus.OutForDelivery;

        if (code is { Length: > 0 } && InTransitEventCodes.Contains(code))
            return TrackingStatus.InTransit;

        if (code is { Length: > 0 } && PreTransitEventCodes.Contains(code))
            return TrackingStatus.PreTransit;

        return TrackingStatus.Unknown;
    }

    private static bool Is(string? value, string expected)
        => string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
}
