using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Parcel.NET.LetterXpress.Letters.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.LetterXpress.Letters.Tests;

public class LetterXpressClientTests
{
    private static readonly byte[] SamplePdf = Encoding.UTF8.GetBytes("%PDF-1.4 sample");

    private static (LetterXpressClient Client, MockHttpMessageHandler Handler) CreateClient(
        string responseBody, HttpStatusCode status = HttpStatusCode.OK, bool testMode = false)
    {
        var handler = new MockHttpMessageHandler(responseBody, status);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.letterxpress.de/v3/") };
        var options = Options.Create(new LetterXpressOptions
        {
            Username = "testuser",
            ApiKey = "secret-key",
            UseTestMode = testMode
        });
        return (new LetterXpressClient(httpClient, options), handler);
    }

    private static JsonElement BodyRoot(MockHttpMessageHandler handler) =>
        JsonDocument.Parse(handler.LastRequestBody!).RootElement;

    [Fact]
    public async Task GetBalanceAsync_ReturnsBalance()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"balance":54.89,"currency":"EUR"}}
            """;
        var (client, _) = CreateClient(json);

        var balance = await client.GetBalanceAsync();

        balance.Amount.ShouldBe(54.89m);
        balance.Currency.ShouldBe("EUR");
    }

    [Fact]
    public async Task GetBalanceAsync_SendsAuthInBody()
    {
        const string json = """{"status":200,"message":"OK","data":{"balance":1,"currency":"EUR"}}""";
        var (client, handler) = CreateClient(json, testMode: true);

        await client.GetBalanceAsync();

        var auth = BodyRoot(handler).GetProperty("auth");
        auth.GetProperty("username").GetString().ShouldBe("testuser");
        auth.GetProperty("apikey").GetString().ShouldBe("secret-key");
        auth.GetProperty("mode").GetString().ShouldBe("test");
    }

    [Fact]
    public async Task GetBalanceAsync_UsesGetMethod()
    {
        const string json = """{"status":200,"message":"OK","data":{"balance":1,"currency":"EUR"}}""";
        var (client, handler) = CreateClient(json);

        await client.GetBalanceAsync();

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/v3/balance");
    }

    [Fact]
    public async Task CreatePrintJobAsync_EncodesFileAndChecksum()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"id":6035143,"shipping":"national","mode":"simplex","color":"4","c4":0,"bank_form":0,"status":"queue","created_at":"2022-08-01 13:38:07","updated_at":"2022-08-01 13:38:07","items":[{"address":"Jim Knopf","pages":1,"amount":0.68,"vat":0.13,"status":"queue"}]}}
            """;
        var (client, handler) = CreateClient(json);

        var job = await client.CreatePrintJobAsync(new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification
            {
                Color = LetterColor.Color,
                Mode = PrintMode.Simplex,
                Shipping = ShippingType.National
            }
        });

        var letter = BodyRoot(handler).GetProperty("letter");
        var expectedBase64 = Convert.ToBase64String(SamplePdf);
        var expectedChecksum = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(expectedBase64))).ToLowerInvariant();

        letter.GetProperty("base64_file").GetString().ShouldBe(expectedBase64);
        letter.GetProperty("base64_file_checksum").GetString().ShouldBe(expectedChecksum);
        letter.GetProperty("specification").GetProperty("color").GetString().ShouldBe("4");
        letter.GetProperty("specification").GetProperty("shipping").GetString().ShouldBe("national");

        job.Id.ShouldBe(6035143);
        job.Status.ShouldBe("queue");
        job.Items.ShouldHaveSingleItem().Amount.ShouldBe(0.68m);
    }

    [Fact]
    public async Task CreatePrintJobAsync_MapsRegisteredAndDispatchDate()
    {
        const string json = """{"status":200,"message":"OK","data":{"id":1,"status":"queue"}}""";
        var (client, handler) = CreateClient(json);

        await client.CreatePrintJobAsync(new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification { Shipping = ShippingType.National },
            Registered = RegisteredMail.EinschreibenEinwurf,
            DispatchDate = new DateOnly(2026, 9, 1),
            Notice = "KdNr. 4711"
        });

        var letter = BodyRoot(handler).GetProperty("letter");
        letter.GetProperty("registered").GetString().ShouldBe("r1");
        letter.GetProperty("dispatch_date").GetString().ShouldBe("2026-09-01");
        letter.GetProperty("notice").GetString().ShouldBe("KdNr. 4711");
    }

    [Fact]
    public async Task GetPriceAsync_SendsPagesAndReturnsPrice()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"price":4.99,"letter":{"specification":{"pages":1,"color":"4","mode":"simplex","shipping":"national"}}}}
            """;
        var (client, handler) = CreateClient(json);

        var price = await client.GetPriceAsync(new PriceRequest
        {
            Pages = 1,
            Color = LetterColor.Color,
            Shipping = ShippingType.National,
            EmailOption = EmailOption.MailSecure
        });

        var spec = BodyRoot(handler).GetProperty("letter").GetProperty("specification");
        spec.GetProperty("pages").GetInt32().ShouldBe(1);
        spec.GetProperty("email_option").GetString().ShouldBe("mailsecure");

        price.Price.ShouldBe(4.99m);
        price.Pages.ShouldBe(1);
        price.Shipping.ShouldBe("national");
    }

    [Fact]
    public async Task ListPrintJobsAsync_BuildsFilterAndPageQuery()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"printjobs":[{"id":1,"status":"hold","c4":0,"bank_form":0,"items":[]}],"pagination":{"total":7,"count":7,"current_page":1,"last_page":1,"per_page":15}}}
            """;
        var (client, handler) = CreateClient(json);

        var result = await client.ListPrintJobsAsync(PrintJobFilter.Hold, page: 2);

        handler.LastRequest!.RequestUri!.Query.ShouldContain("filter=hold");
        handler.LastRequest.RequestUri.Query.ShouldContain("page=2");
        result.Items.ShouldHaveSingleItem().Id.ShouldBe(1);
        result.Pagination!.Total.ShouldBe(7);
    }

    [Fact]
    public async Task DeletePrintJobAsync_UsesDeleteMethod()
    {
        const string json = """{"status":200,"message":"Print job deleted successfully"}""";
        var (client, handler) = CreateClient(json);

        await client.DeletePrintJobAsync(6035143);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/v3/printjobs/6035143");
    }

    [Fact]
    public async Task CreateEmailJobAsync_SingleObject_ReturnsOneEmailJob()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"id":3096,"email_receiver":"max@letterxpress.email","email_option":"mailplus","amount":0.1,"vat":0.02,"status":"queue","subject":"Ihre Rechnung"}}
            """;
        var (client, _) = CreateClient(json);

        var result = await client.CreateEmailJobAsync(new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification(),
            EmailLetter = new EmailLetterOptions { EmailOption = EmailOption.MailPlus, EmailReceiver = "max@letterxpress.email" }
        });

        var job = result.EmailJobs.ShouldHaveSingleItem();
        job.Id.ShouldBe(3096);
        job.EmailOption.ShouldBe("mailplus");
        result.PrintJobs.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateEmailJobAsync_Array_ReturnsMultipleEmailJobs()
    {
        const string json = """
            {"status":200,"message":"OK","data":[{"id":3813,"email_receiver":"a@x.de","email_option":"maildirect","amount":0.05,"vat":0.01,"status":"queue"},{"id":3814,"email_receiver":"b@x.de","email_option":"maildirect","amount":0.05,"vat":0.01,"status":"queue"}]}
            """;
        var (client, _) = CreateClient(json);

        var result = await client.CreateEmailJobAsync(new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification()
        });

        result.EmailJobs.Count.ShouldBe(2);
        result.EmailJobs[1].Id.ShouldBe(3814);
    }

    [Fact]
    public async Task CreateEmailJobAsync_CombinedPrintAndEmail_ReturnsBoth()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"printjobs":[{"id":138606,"status":"queue","c4":0,"bank_form":0,"items":[]}],"emailjobs":[{"id":3837,"email_receiver":"a@x.de","email_option":"mailplus","amount":0.1,"vat":0.02,"status":"queue","printjob_id":138606}]}}
            """;
        var (client, _) = CreateClient(json);

        var result = await client.CreateEmailJobAsync(new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification()
        });

        result.PrintJobs.ShouldHaveSingleItem().Id.ShouldBe(138606);
        var job = result.EmailJobs.ShouldHaveSingleItem();
        job.PrintJobId.ShouldBe(138606);
    }

    [Fact]
    public async Task GetInvoiceAsync_ReturnsInvoiceWithPdf()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"id":25402,"amount":0.67,"vat":0.13,"invoice_date":"2020-05-31","base64_data":"xxxxxxxx"}}
            """;
        var (client, _) = CreateClient(json);

        var invoice = await client.GetInvoiceAsync(25402);

        invoice.Id.ShouldBe(25402);
        invoice.InvoiceDate.ShouldBe(new DateOnly(2020, 5, 31));
        invoice.Base64Data.ShouldBe("xxxxxxxx");
    }

    [Fact]
    public async Task ListTransactionsAsync_MapsItems()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"transactions":[{"amount":-1.04,"currency":"EUR","description":"Farbe","created_at":"2022-09-21 15:32:45"}],"pagination":{"total":62,"count":15,"current_page":1,"last_page":5,"per_page":15,"next_page_url":"http://api.letterxpress.de/v3/transactions?page=2"}}}
            """;
        var (client, handler) = CreateClient(json);

        var result = await client.ListTransactionsAsync(TransactionFilter.PrintJobs);

        handler.LastRequest!.RequestUri!.Query.ShouldContain("filter=printjobs");
        var item = result.Items.ShouldHaveSingleItem();
        item.Amount.ShouldBe(-1.04m);
        // Timestamps are German local time (Europe/Berlin). September => CEST (UTC+2).
        item.CreatedAt.ShouldBe(new DateTimeOffset(2022, 9, 21, 15, 32, 45, TimeSpan.FromHours(2)));
        result.Pagination!.NextPageUrl.ShouldNotBeNull();
    }

    [Fact]
    public async Task SendAsync_SetsContentTypeWithoutCharset()
    {
        // The live API rejects "application/json; charset=utf-8" with HTTP 400.
        const string json = """{"status":200,"message":"OK","data":{"balance":1,"currency":"EUR"}}""";
        var (client, handler) = CreateClient(json);

        await client.GetBalanceAsync();

        handler.LastRequest!.Content!.Headers.ContentType!.ToString().ShouldBe("application/json");
    }

    [Fact]
    public async Task UnauthorizedResponse_ThrowsLetterXpressException()
    {
        const string json = """{"message":"Unauthorized."}""";
        var (client, _) = CreateClient(json, HttpStatusCode.Unauthorized);

        var ex = await Should.ThrowAsync<LetterXpressException>(() => client.GetBalanceAsync());

        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        ex.Message.ShouldContain("Unauthorized");
    }

    [Fact]
    public async Task LiveMode_SendsLiveInAuth()
    {
        const string json = """{"status":200,"message":"OK","data":{"balance":1,"currency":"EUR"}}""";
        var (client, handler) = CreateClient(json, testMode: false);

        await client.GetBalanceAsync();

        BodyRoot(handler).GetProperty("auth").GetProperty("mode").GetString().ShouldBe("live");
    }
}
