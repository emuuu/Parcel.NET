using System.Net;
using System.Text;
using NSubstitute;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.EPost.Internal;
using Parcel.NET.Dhl.EPost.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.EPost.Tests;

public class DhlEPostAuthHandlerTests
{
    private static readonly Uri BaseUri = new("https://api.epost.docuguide.com/");

    private static DhlOptions Options() => new()
    {
        ApiKey = "unused",
        EPostVendorId = "VENDOR1",
        EPostEkp = "5012345678",
        EPostSecret = "secret-xyz",
        EPostPassword = "pw"
    };

    /// <summary>Wires the full stack: client -> auth handler -> api handler, with a token service backed by a login handler.</summary>
    private static DhlEPostClient CreateClient(RecordingHandler api, RecordingHandler login)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(login));
        var tokenService = new DhlEPostTokenService(Microsoft.Extensions.Options.Options.Create(Options()), factory);
        var authHandler = new DhlEPostAuthHandler(tokenService) { InnerHandler = api };
        return new DhlEPostClient(new HttpClient(authHandler) { BaseAddress = BaseUri });
    }

    private static EPostLetterRequest SampleLetter() => new()
    {
        FileName = "a.pdf",
        PdfContent = Encoding.ASCII.GetBytes("%PDF body"),
        AddressLine1 = "Max Mustermann",
        ZipCode = "53113",
        City = "Bonn"
    };

    [Fact]
    public async Task SendAsync_AddsBearerToken()
    {
        var login = new RecordingHandler().Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""");
        var api = new RecordingHandler().Enqueue(HttpStatusCode.OK, """{"letterID":1,"fileName":"a","statusID":4}""");
        var client = CreateClient(api, login);

        await client.GetLetterStatusAsync(1);

        api.CallCount.ShouldBe(1);
        api.AuthParameters[0].ShouldBe("jwt-1");
        login.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task HealthCheck_IsCalledWithoutTokenOrLogin()
    {
        var login = new RecordingHandler(); // must NOT be hit
        var api = new RecordingHandler().Enqueue(
            HttpStatusCode.OK, """{"level":"Info","code":"I501","description":"OK"}""");
        var client = CreateClient(api, login);

        var healthy = await client.HealthCheckAsync();

        healthy.ShouldBeTrue();
        api.CallCount.ShouldBe(1);
        api.AuthParameters[0].ShouldBeNull(); // no Authorization header on HealthCheck
        login.CallCount.ShouldBe(0);          // no login performed
    }

    [Fact]
    public async Task SendAsync_On401_RefreshesTokenAndRetriesOnce()
    {
        var login = new RecordingHandler()
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""")
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-2"}""");
        var api = new RecordingHandler()
            .Enqueue(HttpStatusCode.Unauthorized)
            .Enqueue(HttpStatusCode.OK, """{"letterID":1,"fileName":"a","statusID":4}""");
        var client = CreateClient(api, login);

        var status = await client.GetLetterStatusAsync(1);

        status.Stage.ShouldBe(EPostLetterStage.Dispatched);
        api.CallCount.ShouldBe(2);
        api.AuthParameters[0].ShouldBe("jwt-1");
        api.AuthParameters[1].ShouldBe("jwt-2");
        login.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task SendAsync_On401_ReplaysRequestBody()
    {
        var login = new RecordingHandler()
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""")
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-2"}""");
        var api = new RecordingHandler()
            .Enqueue(HttpStatusCode.Unauthorized)
            .Enqueue(HttpStatusCode.OK, """[{"fileName":"a.pdf","letterID":1}]""");
        var client = CreateClient(api, login);

        await client.SubmitLettersAsync([SampleLetter()]);

        var expectedBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF body"));
        api.CallCount.ShouldBe(2);
        api.Bodies[0].ShouldContain(expectedBase64);
        api.Bodies[1].ShouldBe(api.Bodies[0]); // body faithfully replayed on retry
    }

    [Fact]
    public async Task SendAsync_PersistentlyUnauthorized_RetriesOnlyOnce()
    {
        var login = new RecordingHandler()
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""")
            .Enqueue(HttpStatusCode.OK, """{"token":"jwt-2"}""");
        var api = new RecordingHandler()
            .Enqueue(HttpStatusCode.Unauthorized)
            .Enqueue(HttpStatusCode.Unauthorized);
        var client = CreateClient(api, login);

        var ex = await Should.ThrowAsync<ParcelException>(() => client.GetLetterStatusAsync(1));

        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        api.CallCount.ShouldBe(2); // initial + exactly one retry, no infinite loop
        login.CallCount.ShouldBe(2);
    }
}
