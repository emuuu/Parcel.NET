using Parcel.NET.Dhl.Shipping.Internal;
using Parcel.NET.Dhl.Shipping.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Shipping.Tests;

public class DhlShippingErrorParserTests
{
    [Fact]
    public void ParseValidationMessages_MapsErrorsAndWarnings()
    {
        var body = """
            {"items":[{"validationMessages":[
              {"property":"consignee.postalCode","validationMessage":"The postcode is invalid.","validationState":"Error"},
              {"property":"shipper.email","validationMessage":"Email missing.","validationState":"Warning"}]}]}
            """;

        var messages = DhlShippingErrorParser.ParseValidationMessages(body);

        messages.Count.ShouldBe(2);
        messages[0].Property.ShouldBe("consignee.postalCode");
        messages[0].Message.ShouldBe("The postcode is invalid.");
        messages[0].Severity.ShouldBe(ValidationSeverity.Error);
        messages[1].Severity.ShouldBe(ValidationSeverity.Warning);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("""{"status":{"detail":"x"}}""")]
    [InlineData("""{"items":{}}""")]
    [InlineData("""{"items":[null]}""")]
    [InlineData("""{"items":[1,"x",[]]}""")]
    [InlineData("""{"items":[{}]}""")]
    [InlineData("""{"items":[{"validationMessages":null}]}""")]
    [InlineData("""{"items":[{"validationMessages":{}}]}""")]
    [InlineData("""{"items":[{"validationMessages":[null,1,"x"]}]}""")]
    public void ParseValidationMessages_NoValidationEntries_ReturnsEmptyList(string? body)
    {
        DhlShippingErrorParser.ParseValidationMessages(body).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("not json", false)]
    [InlineData("[]", false)]
    [InlineData("{}", true)]
    [InlineData("""{"items":[null]}""", true)]
    public void TryParseValidationMessages_ReportsWhetherBodyIsJsonObject(string? body, bool expected)
    {
        DhlShippingErrorParser.TryParseValidationMessages(body, out var messages).ShouldBe(expected);
        messages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("""{"validationMessage":"m","validationState":"Error"}""", ValidationSeverity.Error)]
    [InlineData("""{"validationMessage":"m","validationState":"error"}""", ValidationSeverity.Error)]
    [InlineData("""{"validationMessage":"m","validationState":"Warning"}""", ValidationSeverity.Warning)]
    [InlineData("""{"validationMessage":"m","validationState":"Info"}""", ValidationSeverity.Warning)]
    [InlineData("""{"validationMessage":"m","validationState":1}""", ValidationSeverity.Warning)]
    [InlineData("""{"validationMessage":"m","validationState":null}""", ValidationSeverity.Warning)]
    [InlineData("""{"validationMessage":"m"}""", ValidationSeverity.Warning)]
    [InlineData("""{"validationMessage":"m","validationState":"\uDC00"}""", ValidationSeverity.Warning)]
    public void ParseValidationMessages_MapsSeverity(string entry, ValidationSeverity expected)
    {
        var messages = DhlShippingErrorParser.ParseValidationMessages($$"""{"items":[{"validationMessages":[{{entry}}]}]}""");

        messages.Count.ShouldBe(1);
        messages[0].Severity.ShouldBe(expected);
        messages[0].Message.ShouldBe("m");
    }

    [Theory]
    [InlineData("""{"validationState":"Error"}""")]
    [InlineData("""{"validationMessage":42,"validationState":"Error"}""")]
    [InlineData("""{"validationMessage":"","validationState":"Error"}""")]
    [InlineData("""{"validationMessage":"\uD800","validationState":"Error"}""")]
    public void ParseValidationMessages_MissingOrInvalidText_UsesPlaceholder(string entry)
    {
        var messages = DhlShippingErrorParser.ParseValidationMessages($$"""{"items":[{"validationMessages":[{{entry}}]}]}""");

        messages.Count.ShouldBe(1);
        messages[0].Message.ShouldBe("Unknown validation error");
        messages[0].Severity.ShouldBe(ValidationSeverity.Error);
    }

    [Fact]
    public void ParseValidationMessages_NonStringProperty_KeepsMessageWithoutProperty()
    {
        var body = """{"items":[{"validationMessages":[{"property":7,"validationMessage":"m","validationState":"Error"}]}]}""";

        var messages = DhlShippingErrorParser.ParseValidationMessages(body);

        messages.Count.ShouldBe(1);
        messages[0].Property.ShouldBeNull();
    }
}
