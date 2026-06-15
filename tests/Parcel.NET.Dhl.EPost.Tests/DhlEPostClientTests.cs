using System.Net;
using System.Text;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.EPost.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.EPost.Tests;

public class DhlEPostClientTests
{
    private static readonly Uri BaseUri = new("https://api.epost.docuguide.com/");

    private static DhlEPostClient CreateClient(RecordingHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = BaseUri });

    private static EPostLetterRequest SampleLetter() => new()
    {
        FileName = "invoice-001.pdf",
        PdfContent = Encoding.ASCII.GetBytes("%PDF-1.4 test"),
        AddressLine1 = "Max Mustermann",
        AddressLine3 = "Hauptstr. 1",
        ZipCode = "53113",
        City = "Bonn"
    };

    // --- SubmitLettersAsync ---

    [Fact]
    public async Task SubmitLettersAsync_PostsToLetterEndpoint_AndMapsIdents()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            """[{"fileName":"invoice-001.pdf","letterID":43556780}]""");
        var client = CreateClient(handler);

        var results = await client.SubmitLettersAsync([SampleLetter()]);

        handler.Methods[0].ShouldBe(HttpMethod.Post);
        handler.Urls[0].ShouldEndWith("api/Letter");
        results.Count.ShouldBe(1);
        results[0].FileName.ShouldBe("invoice-001.pdf");
        results[0].LetterId.ShouldBe(43556780L);
    }

    [Fact]
    public async Task SubmitLettersAsync_EncodesPdfAsBase64()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.OK, """[{"fileName":"invoice-001.pdf","letterID":1}]""");
        var client = CreateClient(handler);

        await client.SubmitLettersAsync([SampleLetter()]);

        var expectedBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF-1.4 test"));
        handler.Bodies[0].ShouldContain(expectedBase64);
        handler.Bodies[0].ShouldContain("\"fileName\":\"invoice-001.pdf\"");
    }

    [Fact]
    public async Task SubmitLettersAsync_MapsRegisteredLetterAndFlags()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.OK, """[{"fileName":"x","letterID":1}]""");
        var client = CreateClient(handler);

        var letter = SampleLetter();
        letter.RegisteredLetter = RegisteredLetterType.Einwurf;
        letter.Color = true;
        letter.Test = true;
        letter.TestEMail = "qa@example.com";

        await client.SubmitLettersAsync([letter]);

        handler.Bodies[0].ShouldContain("Einwurf Einschreiben");
        handler.Bodies[0].ShouldContain("\"isColor\":true");
        handler.Bodies[0].ShouldContain("\"testFlag\":true");
        handler.Bodies[0].ShouldContain("qa@example.com");
    }

    [Fact]
    public async Task SubmitLettersAsync_OmitsUnsetOptionalFlags()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.OK, """[{"fileName":"x","letterID":1}]""");
        var client = CreateClient(handler);

        await client.SubmitLettersAsync([SampleLetter()]);

        // Unset bool options map to null and are omitted (WhenWritingNull).
        handler.Bodies[0].ShouldNotContain("isColor");
        handler.Bodies[0].ShouldNotContain("testFlag");
        handler.Bodies[0].ShouldNotContain("registeredLetter");
    }

    [Fact]
    public async Task SubmitLettersAsync_SerializesBatchIdAsInteger()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.OK, """[{"fileName":"x","letterID":1}]""");
        var client = CreateClient(handler);

        var letter = SampleLetter();
        letter.BatchId = 12345;

        await client.SubmitLettersAsync([letter]);

        handler.Bodies[0].ShouldContain("\"batchID\":12345");
        handler.Bodies[0].ShouldNotContain("\"batchID\":\"12345\""); // not a JSON string
    }

    [Theory]
    [InlineData("FileName")]
    [InlineData("AddressLine1")]
    [InlineData("ZipCode")]
    [InlineData("City")]
    public async Task SubmitLettersAsync_MissingRequiredField_Throws(string field)
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        var letter = SampleLetter();
        switch (field)
        {
            case "FileName": letter.FileName = "  "; break;
            case "AddressLine1": letter.AddressLine1 = ""; break;
            case "ZipCode": letter.ZipCode = ""; break;
            case "City": letter.City = "  "; break;
        }

        await Should.ThrowAsync<ArgumentException>(() => client.SubmitLettersAsync([letter]));
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SubmitLettersAsync_PdfOver20Mb_Throws()
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        var letter = SampleLetter();
        letter.PdfContent = new byte[(20 * 1024 * 1024) + 1];

        var ex = await Should.ThrowAsync<ArgumentException>(() => client.SubmitLettersAsync([letter]));
        ex.Message.ShouldContain("20 MB");
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SubmitLettersAsync_RequestOver300Mb_Throws()
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        // 16 x 20 MB = 320 MB > 300 MB. Reuse one buffer: the preflight only reads Length, so no 320 MB allocation.
        var shared = new byte[20 * 1024 * 1024];
        var letters = Enumerable.Range(0, 16).Select(i => new EPostLetterRequest
        {
            FileName = $"f{i}.pdf",
            PdfContent = shared,
            AddressLine1 = "Max Mustermann",
            ZipCode = "53113",
            City = "Bonn"
        }).ToArray();

        var ex = await Should.ThrowAsync<ArgumentException>(() => client.SubmitLettersAsync(letters));
        ex.Message.ShouldContain("300 MB");
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SubmitLettersAsync_UnknownRegisteredLetterType_Throws()
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        var letter = SampleLetter();
        letter.RegisteredLetter = (RegisteredLetterType)99;

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => client.SubmitLettersAsync([letter]));
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SubmitLettersAsync_ApiError_SurfacesErrorCodeAndDescription()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.BadRequest,
            """{"level":"Error","code":"E101","description":"Ungültige Sendungsnummer"}""");
        var client = CreateClient(handler);

        var ex = await Should.ThrowAsync<ParcelException>(() => client.SubmitLettersAsync([SampleLetter()]));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.ErrorCode.ShouldBe("E101");
        ex.Message.ShouldContain("E101");
        ex.Message.ShouldContain("Ungültige Sendungsnummer");
    }

    [Fact]
    public async Task SubmitLettersAsync_Empty_DoesNotCallApi()
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        var results = await client.SubmitLettersAsync([]);

        results.ShouldBeEmpty();
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SubmitLettersAsync_ApiError_ThrowsParcelException()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.BadRequest,
            """{"level":"Error","code":"E101","description":"Ungültige Sendungsnummer"}""");
        var client = CreateClient(handler);

        var ex = await Should.ThrowAsync<ParcelException>(() => client.SubmitLettersAsync([SampleLetter()]));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // --- GetLetterStatusAsync ---

    [Theory]
    [InlineData(1, EPostLetterStage.Accepted)]
    [InlineData(2, EPostLetterStage.Processing)]
    [InlineData(3, EPostLetterStage.HandedToPrintCentre)]
    [InlineData(4, EPostLetterStage.Dispatched)]
    [InlineData(99, EPostLetterStage.Error)]
    [InlineData(7, EPostLetterStage.Unknown)]
    public async Task GetLetterStatusAsync_MapsStage(int statusId, EPostLetterStage expected)
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            $$"""{"letterID":42,"fileName":"a.pdf","statusID":{{statusId}}}""");
        var client = CreateClient(handler);

        var status = await client.GetLetterStatusAsync(42);

        handler.Methods[0].ShouldBe(HttpMethod.Get);
        handler.Urls[0].ShouldEndWith("api/Letter/42");
        status.Stage.ShouldBe(expected);
        status.RawStatusId.ShouldBe(statusId);
        status.LetterId.ShouldBe(42L);
    }

    [Fact]
    public async Task GetLetterStatusAsync_Error_PopulatesMessages()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            """
            {"letterID":42,"fileName":"a.pdf","statusID":99,
             "errorList":[{"level":"Error","code":"E500","description":"PDF konnte nicht gedruckt werden"}]}
            """);
        var client = CreateClient(handler);

        var status = await client.GetLetterStatusAsync(42);

        status.Stage.ShouldBe(EPostLetterStage.Error);
        status.Messages.Count.ShouldBe(1);
        status.Messages[0].Code.ShouldBe("E500");
        status.Messages[0].Level.ShouldBe("Error");
    }

    // --- GetLetterStatusesAsync ---

    [Fact]
    public async Task GetLetterStatusesAsync_PostsIdsToStatusQuery()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            """[{"letterID":1,"fileName":"a","statusID":4},{"letterID":2,"fileName":"b","statusID":2}]""");
        var client = CreateClient(handler);

        var statuses = await client.GetLetterStatusesAsync([1L, 2L]);

        handler.Methods[0].ShouldBe(HttpMethod.Post);
        handler.Urls[0].ShouldEndWith("api/Letter/StatusQuery");
        handler.Bodies[0].ShouldBe("[1,2]");
        statuses.Count.ShouldBe(2);
        statuses[0].Stage.ShouldBe(EPostLetterStage.Dispatched);
        statuses[1].Stage.ShouldBe(EPostLetterStage.Processing);
    }

    [Fact]
    public async Task GetLetterStatusesAsync_Empty_DoesNotCallApi()
    {
        var handler = new RecordingHandler();
        var client = CreateClient(handler);

        var statuses = await client.GetLetterStatusesAsync([]);

        statuses.ShouldBeEmpty();
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetLetterStatusesAsync_RateLimited_ThrowsWithHint()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.TooManyRequests);
        var client = CreateClient(handler);

        var ex = await Should.ThrowAsync<ParcelException>(() => client.GetLetterStatusesAsync([1L]));
        ex.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        ex.Message.ShouldContain("rate limit");
    }

    // --- HealthCheckAsync ---

    [Fact]
    public async Task HealthCheckAsync_ReturnsTrue_OnSuccess()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.OK);
        var client = CreateClient(handler);

        (await client.HealthCheckAsync()).ShouldBeTrue();
        handler.Urls[0].ShouldEndWith("api/Login/HealthCheck");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsTrue_OnInfoStatus()
    {
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            """{"level":"Info","code":"I501","description":"API-Status: OK"}""");
        var client = CreateClient(handler);

        (await client.HealthCheckAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsFalse_OnInactiveErrorStatus()
    {
        // 200 OK but the Error object reports E501 (API inactive).
        var handler = new RecordingHandler().Enqueue(
            HttpStatusCode.OK,
            """{"level":"Error","code":"E501","description":"API-Status: Inaktiv"}""");
        var client = CreateClient(handler);

        (await client.HealthCheckAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsFalse_OnServiceUnavailable()
    {
        var handler = new RecordingHandler().Enqueue(HttpStatusCode.ServiceUnavailable);
        var client = CreateClient(handler);

        (await client.HealthCheckAsync()).ShouldBeFalse();
    }
}
