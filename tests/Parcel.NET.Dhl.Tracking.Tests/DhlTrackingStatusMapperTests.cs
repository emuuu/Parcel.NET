using Parcel.NET.Abstractions.Models;
using Parcel.NET.Dhl.Tracking.Internal;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Tracking.Tests;

public class DhlTrackingStatusMapperTests
{
    // Rule 1: delivery flag or ICE DLVRD
    [Theory]
    [InlineData("1", "false", null, null)]
    [InlineData("1", "true", "RETRN", "BV")]
    [InlineData("0", "false", "DLVRD", "ZU")]
    [InlineData("0", "false", "dlvrd", null)]
    [InlineData("0", "false", "DLVRD", "GT")]
    [InlineData(" 1 ", null, null, null)]
    public void Map_Rule1_Delivered(string? flag, string? returned, string? ice, string? code)
        => DhlTrackingStatusMapper.Map(flag, returned, ice, code).ShouldBe(TrackingStatus.Delivered);

    // Rule 2: return shipment
    [Theory]
    [InlineData("0", "true", null, null)]
    [InlineData("0", "TRUE", "DLVRF", "ZN")]
    [InlineData("0", "false", "RETRN", "ZN")]
    [InlineData("0", null, "retrn", null)]
    public void Map_Rule2_Returned(string? flag, string? returned, string? ice, string? code)
        => DhlTrackingStatusMapper.Map(flag, returned, ice, code).ShouldBe(TrackingStatus.Returned);

    // Rule 3: exception ICE events or standard event codes ZN / BV / AN
    [Theory]
    [InlineData("DLVRF", null)]
    [InlineData("NTDEL", "ZN")]
    [InlineData("DMGDS", "BV")]
    [InlineData("DSPSD", "ZU")]
    [InlineData("EXPHD", "PO")]
    [InlineData("LDEXC", "PO")]
    [InlineData("MSRTD", "PO")]
    [InlineData("MVTEX", "PO")]
    [InlineData("NTPUP", "AN")]
    [InlineData("CANCL", "BV")]
    [InlineData(null, "ZN")]
    [InlineData(null, "BV")]
    [InlineData(null, "AN")]
    [InlineData("LDTMV", "zn")]
    public void Map_Rule3_Exception(string? ice, string? code)
        => DhlTrackingStatusMapper.Map("0", "false", ice, code).ShouldBe(TrackingStatus.Exception);

    // Rule 4: ZU
    [Theory]
    [InlineData("ZU")]
    [InlineData("zu")]
    [InlineData(" ZU ")]
    public void Map_Rule4_Delivered(string code)
        => DhlTrackingStatusMapper.Map("0", "false", "SRTED", code).ShouldBe(TrackingStatus.Delivered);

    // Rule 5: PO
    [Theory]
    [InlineData("SRTED", "PO")]
    [InlineData(null, "PO")]
    [InlineData("HNDDE", "po")]
    public void Map_Rule5_OutForDelivery(string? ice, string code)
        => DhlTrackingStatusMapper.Map("0", "false", ice, code).ShouldBe(TrackingStatus.OutForDelivery);

    // Rule 6: other transit codes, including branch (ZF) and storage (LA)
    [Theory]
    [InlineData("ES")]
    [InlineData("AE")]
    [InlineData("AA")]
    [InlineData("EE")]
    [InlineData("NB")]
    [InlineData("ZO")]
    [InlineData("ZF")]
    [InlineData("LA")]
    [InlineData("GT")]
    public void Map_Rule6_InTransit(string code)
        => DhlTrackingStatusMapper.Map("0", "false", "LDTMV", code).ShouldBe(TrackingStatus.InTransit);

    // Rule 7: announced
    [Theory]
    [InlineData("VA")]
    [InlineData("DD")]
    [InlineData("ST")]
    public void Map_Rule7_PreTransit(string code)
        => DhlTrackingStatusMapper.Map("0", "false", "PARCV", code).ShouldBe(TrackingStatus.PreTransit);

    // Rule 8: everything else
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("  ", " ")]
    [InlineData("LDTMV", null)]
    [InlineData("LDTMV", "XX")]
    public void Map_Rule8_Unknown(string? ice, string? code)
        => DhlTrackingStatusMapper.Map("0", "false", ice, code).ShouldBe(TrackingStatus.Unknown);

    // The first matching rule wins.
    [Fact]
    public void Map_ExceptionIceBeatsDeliveredCode() // DSPSD + ZU
        => DhlTrackingStatusMapper.Map("0", "false", "DSPSD", "ZU").ShouldBe(TrackingStatus.Exception);

    [Fact]
    public void Map_DeliveredFlagBeatsReturnAndException()
        => DhlTrackingStatusMapper.Map("1", "true", "NTDEL", "ZN").ShouldBe(TrackingStatus.Delivered);

    [Fact]
    public void Map_ReturnBeatsException()
        => DhlTrackingStatusMapper.Map("0", "true", "NTDEL", "ZN").ShouldBe(TrackingStatus.Returned);

    [Fact]
    public void Map_ExceptionCodeBeatsOutForDeliveryIce()
        => DhlTrackingStatusMapper.Map("0", "false", "SRTED", "ZN").ShouldBe(TrackingStatus.Exception);

    [Fact]
    public void Map_DeliveredCodeBeatsOutForDelivery()
        => DhlTrackingStatusMapper.Map("0", "false", null, "ZU").ShouldBe(TrackingStatus.Delivered);

    // Each exception ICE event counts on its own, independent of the standard event code.
    [Theory]
    [InlineData("DLVRF", "PO")]
    [InlineData("NTDEL", "PO")]
    [InlineData("DMGDS", "AA")]
    [InlineData("DSPSD", "PO")]
    [InlineData("EXPHD", "ZF")]
    [InlineData("LDEXC", "PO")]
    [InlineData("MSRTD", "PO")]
    [InlineData("MVTEX", "PO")]
    [InlineData("NTPUP", "DD")]
    [InlineData("CANCL", "VA")]
    public void Map_EachExceptionIceEvent_WinsOverTransitCodes(string ice, string code)
        => DhlTrackingStatusMapper.Map("0", "false", ice, code).ShouldBe(TrackingStatus.Exception);

    // Every ICE/RIC/TTPRO combination of the official list yields a defined status
    // (no exception, and TTPRO codes of the list never end up as Unknown).
    [Theory]
    [InlineData("ES")]
    [InlineData("AA")]
    [InlineData("AE")]
    [InlineData("AN")]
    [InlineData("BV")]
    [InlineData("DD")]
    [InlineData("EE")]
    [InlineData("GT")]
    [InlineData("LA")]
    [InlineData("NB")]
    [InlineData("PO")]
    [InlineData("ST")]
    [InlineData("VA")]
    [InlineData("ZF")]
    [InlineData("ZN")]
    [InlineData("ZO")]
    [InlineData("ZU")]
    public void Map_EveryStandardEventCodeOfTheOfficialList_IsKnown(string code)
        => DhlTrackingStatusMapper.Map("0", "false", null, code).ShouldNotBe(TrackingStatus.Unknown);
}
