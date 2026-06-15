using System.Net;
using Microsoft.Extensions.Options;
using NSubstitute;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.EPost.Internal;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.EPost.Tests;

public class DhlEPostTokenServiceTests
{
    private static DhlOptions Options() => new()
    {
        ApiKey = "unused",
        EPostVendorId = "VENDOR1",
        EPostEkp = "5012345678",
        EPostSecret = "secret-xyz",
        EPostPassword = "pw"
    };

    private static (DhlEPostTokenService Service, RecordingHandler Login) CreateService(DhlOptions? options = null)
    {
        var login = new RecordingHandler();
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(login));
        var service = new DhlEPostTokenService(Microsoft.Extensions.Options.Options.Create(options ?? Options()), factory);
        return (service, login);
    }

    [Fact]
    public async Task GetTokenAsync_PostsLoginAndReturnsToken()
    {
        var (service, login) = CreateService();
        login.Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""");

        var token = await service.GetTokenAsync();

        token.ShouldBe("jwt-1");
        login.Methods[0].ShouldBe(HttpMethod.Post);
        login.Urls[0].ShouldEndWith("api/Login");
        login.Bodies[0].ShouldContain("\"vendorID\":\"VENDOR1\"");
        login.Bodies[0].ShouldContain("\"ekp\":\"5012345678\"");
        login.Bodies[0].ShouldContain("\"secret\":\"secret-xyz\"");
        login.Bodies[0].ShouldContain("\"password\":\"pw\"");
    }

    [Fact]
    public async Task GetTokenAsync_CachesTokenAcrossCalls()
    {
        var (service, login) = CreateService();
        login.Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""");

        var first = await service.GetTokenAsync();
        var second = await service.GetTokenAsync();

        first.ShouldBe("jwt-1");
        second.ShouldBe("jwt-1");
        login.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task Invalidate_ForcesReLogin()
    {
        var (service, login) = CreateService();
        login.Enqueue(HttpStatusCode.OK, """{"token":"jwt-1"}""")
             .Enqueue(HttpStatusCode.OK, """{"token":"jwt-2"}""");

        var first = await service.GetTokenAsync();
        service.Invalidate();
        var second = await service.GetTokenAsync();

        first.ShouldBe("jwt-1");
        second.ShouldBe("jwt-2");
        login.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetTokenAsync_LoginFails_ThrowsParcelException()
    {
        var (service, login) = CreateService();
        login.Enqueue(HttpStatusCode.Unauthorized, """{"level":"Error","code":"E001","description":"bad credentials"}""");

        var ex = await Should.ThrowAsync<ParcelException>(() => service.GetTokenAsync());
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        ex.ErrorCode.ShouldBe("E001");
        ex.Message.ShouldContain("E001");
        ex.Message.ShouldContain("bad credentials");
    }

    [Fact]
    public async Task GetTokenAsync_MissingVendorId_Throws()
    {
        var options = Options();
        options.EPostVendorId = null;
        var (service, _) = CreateService(options);

        await Should.ThrowAsync<InvalidOperationException>(() => service.GetTokenAsync());
    }
}
