using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Parcel.NET.LetterXpress.Letters.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.LetterXpress.Letters.Tests;

public class LetterXpressClientCoverageTests
{
    private static readonly byte[] SamplePdf = Encoding.UTF8.GetBytes("%PDF-1.4 sample");

    private static (LetterXpressClient Client, MockHttpMessageHandler Handler) CreateClient(
        string responseBody, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new MockHttpMessageHandler(responseBody, status);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.letterxpress.de/v3/") };
        var options = Options.Create(new LetterXpressOptions { Username = "u", ApiKey = "k" });
        return (new LetterXpressClient(httpClient, options), handler);
    }

    private static JsonElement BodyRoot(MockHttpMessageHandler handler) =>
        JsonDocument.Parse(handler.LastRequestBody!).RootElement;

    private static LetterRequest BasicLetter() => new()
    {
        File = SamplePdf,
        Specification = new LetterSpecification { Shipping = ShippingType.National }
    };

    private const string OkPrintJob = """{"status":200,"message":"OK","data":{"id":1,"status":"queue"}}""";

    // ---- Validation -----------------------------------------------------

    [Fact]
    public async Task CreatePrintJobAsync_EmptyFile_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = new LetterRequest { File = [], Specification = new LetterSpecification() };

        await Should.ThrowAsync<ArgumentException>(() => client.CreatePrintJobAsync(req));
    }

    [Fact]
    public async Task CreateEmailJobAsync_SerialAndEmailCombined_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.SerialLetter = new SerialLetterOptions { PagesSeparatorType = SeparatorType.Number, PagesSeparatorValue = "2" };
        req.EmailLetter = new EmailLetterOptions { EmailOption = EmailOption.MailDirect, EmailReceiver = "a@x.de" };

        await Should.ThrowAsync<ArgumentException>(() => client.CreateEmailJobAsync(req));
    }

    [Fact]
    public async Task CreatePrintJobAsync_RegisteredInternational_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = new LetterRequest
        {
            File = SamplePdf,
            Specification = new LetterSpecification { Shipping = ShippingType.International },
            Registered = RegisteredMail.Einschreiben
        };

        await Should.ThrowAsync<ArgumentException>(() => client.CreatePrintJobAsync(req));
    }

    [Fact]
    public async Task GetPriceAsync_AutoShipping_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);

        await Should.ThrowAsync<ArgumentException>(() =>
            client.GetPriceAsync(new PriceRequest { Pages = 1, Shipping = ShippingType.Auto }));
    }

    [Fact]
    public async Task GetPriceAsync_NonPositivePages_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);

        await Should.ThrowAsync<ArgumentException>(() =>
            client.GetPriceAsync(new PriceRequest { Pages = 0, Shipping = ShippingType.National }));
    }

    [Fact]
    public async Task CreatePrintJobAsync_SerialNumberWithNonNumericValue_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.SerialLetter = new SerialLetterOptions { PagesSeparatorType = SeparatorType.Number, PagesSeparatorValue = "abc" };

        await Should.ThrowAsync<ArgumentException>(() => client.CreatePrintJobAsync(req));
    }

    [Fact]
    public async Task CreatePrintJobAsync_PastDispatchDate_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.DispatchDate = new DateOnly(2000, 1, 1);

        await Should.ThrowAsync<ArgumentException>(() => client.CreatePrintJobAsync(req));
    }

    [Fact]
    public async Task CreatePrintJobAsync_OversizedAttachmentsTotal_Throws()
    {
        var (client, _) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        // Main file is tiny, but an attachment pushes the request over the 50 MB limit.
        req.Attachments = [new byte[51 * 1024 * 1024]];

        await Should.ThrowAsync<ArgumentException>(() => client.CreatePrintJobAsync(req));
    }

    // ---- Serial letter separator serialization --------------------------

    [Fact]
    public async Task CreatePrintJobAsync_SerialNumber_SerializesAsJsonNumber()
    {
        var (client, handler) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.SerialLetter = new SerialLetterOptions { PagesSeparatorType = SeparatorType.Number, PagesSeparatorValue = "3" };

        await client.CreatePrintJobAsync(req);

        var serial = BodyRoot(handler).GetProperty("letter").GetProperty("serial_letter");
        serial.GetProperty("pages_separator_type").GetString().ShouldBe("number");
        var value = serial.GetProperty("pages_separator_value");
        value.ValueKind.ShouldBe(JsonValueKind.Number);
        value.GetInt32().ShouldBe(3);
    }

    [Fact]
    public async Task CreatePrintJobAsync_SerialString_SerializesAsJsonString()
    {
        var (client, handler) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.SerialLetter = new SerialLetterOptions { PagesSeparatorType = SeparatorType.String, PagesSeparatorValue = "TRENN" };

        await client.CreatePrintJobAsync(req);

        var value = BodyRoot(handler).GetProperty("letter").GetProperty("serial_letter").GetProperty("pages_separator_value");
        value.ValueKind.ShouldBe(JsonValueKind.String);
        value.GetString().ShouldBe("TRENN");
    }

    // ---- Complex payload building ---------------------------------------

    [Fact]
    public async Task CreatePrintJobAsync_AttachmentsAndBankForm_Serialized()
    {
        var (client, handler) = CreateClient(OkPrintJob);
        var req = BasicLetter();
        req.Attachments = [Encoding.UTF8.GetBytes("att1")];
        req.BankForm = new BankForm { Included = false, Iban = "DE0001", Amount = "12,00" };

        await client.CreatePrintJobAsync(req);

        var letter = BodyRoot(handler).GetProperty("letter");
        var attachments = letter.GetProperty("base64_attachments");
        attachments.GetArrayLength().ShouldBe(1);
        attachments[0].GetString().ShouldBe(Convert.ToBase64String(Encoding.UTF8.GetBytes("att1")));

        var bank = letter.GetProperty("bank_form");
        bank.GetProperty("bank_form_included").GetInt32().ShouldBe(0);
        bank.GetProperty("iban").GetString().ShouldBe("DE0001");
    }

    // ---- Update partial-spec --------------------------------------------

    [Fact]
    public async Task UpdatePrintJobAsync_NoticeOnly_OmitsSpecification()
    {
        var (client, handler) = CreateClient(OkPrintJob);

        await client.UpdatePrintJobAsync(1, new PrintJobUpdate { Notice = "only notice" });

        var letter = BodyRoot(handler).GetProperty("letter");
        letter.GetProperty("notice").GetString().ShouldBe("only notice");
        letter.TryGetProperty("specification", out _).ShouldBeFalse();
        letter.TryGetProperty("dispatch_date", out _).ShouldBeFalse();
        letter.TryGetProperty("registered", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdatePrintJobAsync_ColorOnly_SendsPartialSpecification()
    {
        var (client, handler) = CreateClient(OkPrintJob);

        await client.UpdatePrintJobAsync(1, new PrintJobUpdate { Color = LetterColor.Color });

        var spec = BodyRoot(handler).GetProperty("letter").GetProperty("specification");
        spec.GetProperty("color").GetString().ShouldBe("4");
        spec.TryGetProperty("mode", out _).ShouldBeFalse();
        spec.TryGetProperty("shipping", out _).ShouldBeFalse();
        spec.TryGetProperty("c4", out _).ShouldBeFalse();
    }

    // ---- Body-level error -----------------------------------------------

    [Fact]
    public async Task SendAsync_BodyLevelStatusError_ThrowsWithStatusCode()
    {
        // HTTP 200 but the body reports an application-level error.
        const string json = """{"status":400,"message":"Bad Request"}""";
        var (client, _) = CreateClient(json, HttpStatusCode.OK);

        var ex = await Should.ThrowAsync<LetterXpressException>(() => client.GetBalanceAsync());

        ex.ErrorCode.ShouldBe("400");
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.RawResponse.ShouldNotBeNull();
        ex.RawResponse!.ShouldContain("Bad Request");
    }

    // ---- Missing endpoint coverage --------------------------------------

    [Fact]
    public async Task GetPrintJobAsync_ReturnsRenderedPdf()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"id":99,"status":"done","c4":0,"bank_form":0,"items":[{"address":"X","pages":1,"amount":0.67,"vat":0.13,"status":"sent","tracking_code":"RC1DE","base64_data":"QUJD"}]}}
            """;
        var (client, handler) = CreateClient(json);

        var job = await client.GetPrintJobAsync(99);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/v3/printjobs/99");
        var item = job.Items.ShouldHaveSingleItem();
        item.TrackingCode.ShouldBe("RC1DE");
        item.Base64Data.ShouldBe("QUJD");
    }

    [Fact]
    public async Task GetEmailJobAsync_MapsNestedPrintJob()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"id":918,"email_receiver":"j@aof.de","email_option":"mailplus","amount":0.1,"vat":0.02,"status":"success","printjob":{"id":138037,"status":"done","c4":0,"bank_form":0,"items":[{"address":"X","pages":1,"amount":0.67,"vat":0.13,"status":"sent"}]}}}
            """;
        var (client, _) = CreateClient(json);

        var job = await client.GetEmailJobAsync(918);

        job.Id.ShouldBe(918);
        job.PrintJob.ShouldNotBeNull();
        job.PrintJob!.Id.ShouldBe(138037);
    }

    [Fact]
    public async Task DeleteEmailJobAsync_UsesDeleteMethod()
    {
        const string json = """{"status":200,"message":"Email job deleted successfully"}""";
        var (client, handler) = CreateClient(json);

        await client.DeleteEmailJobAsync(3902);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/v3/emailjobs/3902");
    }

    [Fact]
    public async Task UpdateEmailJobAsync_SendsNewReceiver()
    {
        const string json = """{"status":200,"message":"OK","data":{"id":3902,"email_receiver":"new@aof.de","email_option":"maildirect","amount":0.05,"vat":0.01,"status":"queue"}}""";
        var (client, handler) = CreateClient(json);

        var job = await client.UpdateEmailJobAsync(3902, "new@aof.de");

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        BodyRoot(handler).GetProperty("email").GetProperty("email_receiver").GetString().ShouldBe("new@aof.de");
        job.EmailReceiver.ShouldBe("new@aof.de");
    }

    [Fact]
    public async Task ListEmailJobsAsync_WithFilter_ParsesAndBuildsQuery()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"emailjobs":[{"id":3093,"email_receiver":"j@aof.de","email_option":"mailplus","amount":0.1,"vat":0.02,"status":"hold"}],"pagination":{"total":1,"count":1,"current_page":1,"last_page":1,"per_page":15,"first_page_url":"https://api.letterxpress.de/v3/emailjobs?page=1","last_page_url":"https://api.letterxpress.de/v3/emailjobs?page=1"}}}
            """;
        var (client, handler) = CreateClient(json);

        var result = await client.ListEmailJobsAsync(EmailJobFilter.Hold);

        handler.LastRequest!.RequestUri!.Query.ShouldContain("filter=hold");
        result.Items.ShouldHaveSingleItem().Id.ShouldBe(3093);
        result.Pagination!.FirstPageUrl.ShouldNotBeNull();
        result.Pagination.LastPageUrl.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListInvoicesAsync_MapsItems()
    {
        const string json = """
            {"status":200,"message":"OK","data":{"invoices":[{"id":25402,"amount":0.67,"vat":0.13,"invoice_date":"2020-05-31"}],"pagination":{"total":1,"count":1,"current_page":1,"last_page":1,"per_page":15}}}
            """;
        var (client, _) = CreateClient(json);

        var result = await client.ListInvoicesAsync();

        var inv = result.Items.ShouldHaveSingleItem();
        inv.Id.ShouldBe(25402);
        inv.InvoiceDate.ShouldBe(new DateOnly(2020, 5, 31));
    }

    [Fact]
    public async Task ListPrintJobsAsync_NoFilterOrPage_HasNoQuery()
    {
        const string json = """{"status":200,"message":"OK","data":{"printjobs":[],"pagination":{"total":0,"count":0,"current_page":1,"last_page":1,"per_page":50}}}""";
        var (client, handler) = CreateClient(json);

        await client.ListPrintJobsAsync();

        handler.LastRequest!.RequestUri!.Query.ShouldBeEmpty();
    }

    // ---- Timezone (DST) -------------------------------------------------

    [Fact]
    public async Task ParseDateTime_WinterDate_UsesCetPlusOne()
    {
        var berlin = TryGetBerlin();
        if (berlin is null)
        {
            Assert.Skip("Europe/Berlin timezone data not available on this machine.");
        }

        const string json = """
            {"status":200,"message":"OK","data":{"transactions":[{"amount":-1.0,"currency":"EUR","description":"x","created_at":"2024-01-15 09:00:00"}],"pagination":{"total":1,"count":1,"current_page":1,"last_page":1,"per_page":15}}}
            """;
        var (client, _) = CreateClient(json);

        var result = await client.ListTransactionsAsync();

        // January => CET (UTC+1).
        result.Items.ShouldHaveSingleItem().CreatedAt
            .ShouldBe(new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.FromHours(1)));
    }

    private static TimeZoneInfo? TryGetBerlin()
    {
        foreach (var id in (string[])["Europe/Berlin", "W. Europe Standard Time"])
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return null;
    }
}
