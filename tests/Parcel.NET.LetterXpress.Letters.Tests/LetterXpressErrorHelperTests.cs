using Parcel.NET.LetterXpress.Internal;
using Shouldly;
using Xunit;

namespace Parcel.NET.LetterXpress.Letters.Tests;

public class LetterXpressErrorHelperTests
{
    private const string ValidationErrorBody =
        """{"status":400,"message":{"letter.registered":["The letter.registered field is prohibited when letter.specification.shipping is auto."]}}""";

    [Fact]
    public void StringMessage_ReturnsMessage()
    {
        LetterXpressErrorHelper.TryParseErrorDetail("""{"status":401,"message":"Unauthorized."}""")
            .ShouldBe("Unauthorized.");
    }

    [Fact]
    public void ObjectMessage_FlattensFieldMessages()
    {
        LetterXpressErrorHelper.TryParseErrorDetail(ValidationErrorBody)
            .ShouldBe("letter.registered: The letter.registered field is prohibited when letter.specification.shipping is auto.");
    }

    [Fact]
    public void ObjectMessage_MultipleFieldsAndMessages_JoinsWithSemicolon()
    {
        const string body = """
            {"status":422,"message":{"letter.base64_file":["The file is required.","The file must be a PDF."],"letter.specification.color":["Invalid color."]}}
            """;

        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(
            "letter.base64_file: The file is required.; The file must be a PDF.; letter.specification.color: Invalid color.");
    }

    [Fact]
    public void ArrayMessage_JoinsEntries()
    {
        LetterXpressErrorHelper.TryParseErrorDetail("""{"message":["First problem.","Second problem."]}""")
            .ShouldBe("First problem.; Second problem.");
    }

    [Fact]
    public void ErrorInsteadOfMessage_ReturnsError()
    {
        LetterXpressErrorHelper.TryParseErrorDetail("""{"error":"Forbidden."}""").ShouldBe("Forbidden.");
    }

    [Fact]
    public void ObjectError_FlattensFieldMessages()
    {
        LetterXpressErrorHelper.TryParseErrorDetail("""{"error":{"auth.apikey":["Invalid API key."]}}""")
            .ShouldBe("auth.apikey: Invalid API key.");
    }

    [Fact]
    public void EmptyMessage_FallsBackToError()
    {
        LetterXpressErrorHelper.TryParseErrorDetail("""{"message":{},"error":"Forbidden."}""").ShouldBe("Forbidden.");
    }

    [Fact]
    public void NullMessage_ReturnsRawBody()
    {
        const string body = """{"status":500,"message":null}""";

        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(body);
    }

    [Fact]
    public void NotJson_ReturnsRawBody()
    {
        const string body = "<html><body>502 Bad Gateway</body></html>";

        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyBody_ReturnsBodyUnchanged(string body)
    {
        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(body);
    }

    [Fact]
    public void JsonWithoutMessageOrError_ReturnsRawBody()
    {
        const string body = """{"status":500,"data":null}""";

        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(body);
    }

    [Fact]
    public void NonObjectRoot_ReturnsRawBody()
    {
        const string body = """["message"]""";

        LetterXpressErrorHelper.TryParseErrorDetail(body).ShouldBe(body);
    }

    [Fact]
    public void UndecodableString_DoesNotThrow()
    {
        // An unpaired surrogate escape is valid JSON but cannot be decoded by GetString().
        const string body = """{"message":"\uD800"}""";

        Should.NotThrow(() => LetterXpressErrorHelper.TryParseErrorDetail(body)).ShouldBe(body);
    }
}
