using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Abstractions.Models;
using Parcel.NET.GoExpress.Shipping.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.GoExpress.Shipping.Tests;

public class GoExpressShippingClientTests
{
    private static readonly GoExpressOptions TestOptions = new()
    {
        Username = "testuser",
        Password = "testpass",
        CustomerId = "50000",
        ResponsibleStation = "HOQ",
        UseSandbox = true
    };

    private static GoExpressShipmentRequest CreateTestRequest(
        GoExpressService service = GoExpressService.ON,
        GoExpressLabelFormat labelFormat = GoExpressLabelFormat.PdfA4,
        bool selfPickup = false,
        bool identCheck = false) => new()
    {
        Service = service,
        LabelFormat = labelFormat,
        SelfPickup = selfPickup,
        IdentCheck = identCheck,
        Pickup = new TimeWindow
        {
            Date = new DateOnly(2025, 8, 1),
            TimeFrom = new TimeOnly(15, 0),
            TimeTill = new TimeOnly(17, 0)
        },
        Shipper = new Address
        {
            Name = "Test AG",
            Street = "Teststraße",
            HouseNumber = "1",
            PostalCode = "53119",
            City = "Bonn",
            CountryCode = "DE"
        },
        Consignee = new Address
        {
            Name = "Test Consignee 1",
            Street = "Empfängerstraße",
            HouseNumber = "10",
            PostalCode = "36272",
            City = "Niederaula",
            CountryCode = "DE"
        },
        Packages =
        [
            new Package { Weight = 5.0, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } },
            new Package { Weight = 5.25, Dimensions = new Dimensions { Length = 40, Width = 30, Height = 20 } }
        ],
        Reference = "Test Reference"
    };

    private static object CreateSuccessResponse() => new
    {
        hwbNumber = "346940009768",
        orderStatus = "New",
        pickupDate = "01.08.2025",
        deliveryDate = "02.08.2025",
        transitInfo = new { datesVerified = "Yes", addressesVerified = "Yes", remarks = "" },
        hwbOrPackageLabel = Convert.ToBase64String("fake-pdf"u8.ToArray()),
        package = new[] { new { barcode = "346940009768001" }, new { barcode = "346940009768002" } }
    };

    private static GoExpressShippingClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci/")
        }, Options.Create(TestOptions));

    private static GoExpressShippingClient CreateClientWithHandler(MockHttpMessageHandler handler) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ws-tst.api.general-overnight.com/external/ci/")
        }, Options.Create(TestOptions));

    private static async Task<JsonDocument> GetRequestBodyAsync(MockHttpMessageHandler handler)
    {
        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        return JsonDocument.Parse(body);
    }

    // --- CreateShipment Basic Tests ---

    [Fact]
    public async Task CreateShipmentAsync_ReturnsShipmentNumber()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });

        var result = await client.CreateShipmentAsync(CreateTestRequest());

        result.ShipmentNumber.ShouldBe("346940009768");
        result.Labels.ShouldNotBeEmpty();
        result.Labels[0].Content.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task CreateShipmentAsync_ReturnsPdfLabel()
    {
        var pdfBytes = "fake-pdf-content"u8.ToArray();
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "GO123",
                orderStatus = "New",
                hwbOrPackageLabel = Convert.ToBase64String(pdfBytes),
                package = new[] { new { barcode = "GO123001" } }
            })
        });

        var result = await client.CreateShipmentAsync(CreateTestRequest());

        result.Labels[0].Format.ShouldBe(LabelFormat.Pdf);
        result.Labels[0].Content.ShouldBe(pdfBytes);
    }

    [Fact]
    public async Task CreateShipmentAsync_ReturnsZplLabel()
    {
        var zplContent = "^XA^FO50,50^ADN,36,20^FDHello^FS^XZ";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "GO123",
                orderStatus = "New",
                hwbOrPackageLabel = zplContent,
                package = new[] { new { barcode = "GO123001" } }
            })
        });

        var request = CreateTestRequest(labelFormat: GoExpressLabelFormat.Zpl);
        var result = await client.CreateShipmentAsync(request);

        result.Labels[0].Format.ShouldBe(LabelFormat.Zpl);
        System.Text.Encoding.UTF8.GetString(result.Labels[0].Content).ShouldBe(zplContent);
    }

    [Fact]
    public async Task CreateShipmentAsync_ApiError_ThrowsShippingException()
    {
        var errorBody = """{"message":"Invalid customer ID."}""";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ShippingException>(
            () => client.CreateShipmentAsync(CreateTestRequest()));

        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.RawResponse.ShouldNotBeNull();
        ex.RawResponse.ShouldContain("Invalid customer ID");
    }

    [Fact]
    public async Task CreateShipmentAsync_NullRequest_ThrowsArgumentNullException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        await Should.ThrowAsync<ArgumentNullException>(() => client.CreateShipmentAsync(null!));
    }

    // --- JSON Structure Tests ---

    [Fact]
    public async Task CreateShipmentAsync_AddressesAreTopLevel_NotInsideShipment()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        root.TryGetProperty("consignorAddress", out _).ShouldBeTrue("consignorAddress should be top-level");
        root.TryGetProperty("consigneeAddress", out _).ShouldBeTrue("consigneeAddress should be top-level");
        root.TryGetProperty("neutralAddress", out _).ShouldBeTrue("neutralAddress should be top-level");
        root.TryGetProperty("packages", out _).ShouldBeTrue("packages should be top-level");
        root.TryGetProperty("label", out _).ShouldBeTrue("label should be top-level");

        var shipment = root.GetProperty("shipment");
        shipment.TryGetProperty("consignorAddress", out _).ShouldBeFalse("addresses should NOT be in shipment");
        shipment.TryGetProperty("consigneeAddress", out _).ShouldBeFalse();
        shipment.TryGetProperty("shipper", out _).ShouldBeFalse();
        shipment.TryGetProperty("consignee", out _).ShouldBeFalse();
        shipment.TryGetProperty("packages", out _).ShouldBeFalse("packages should NOT be in shipment");
    }

    [Fact]
    public async Task CreateShipmentAsync_DateFormat_IsDdMmYyyy()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var pickupDate = doc.RootElement.GetProperty("shipment").GetProperty("pickup").GetProperty("date").GetString();

        pickupDate.ShouldBe("01.08.2025");
    }

    [Fact]
    public async Task CreateShipmentAsync_AddressFieldNames_MatchApi()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var consignor = doc.RootElement.GetProperty("consignorAddress");

        consignor.TryGetProperty("name1", out var name1).ShouldBeTrue("should use name1, not name");
        name1.GetString().ShouldBe("Test AG");
        consignor.TryGetProperty("zipCode", out var zip).ShouldBeTrue("should use zipCode, not postalCode");
        zip.GetString().ShouldBe("53119");
        consignor.TryGetProperty("country", out var country).ShouldBeTrue("should use country, not countryCode");
        country.GetString().ShouldBe("DE");

        // These old field names should NOT exist
        consignor.TryGetProperty("name", out _).ShouldBeFalse("old field 'name' should not exist");
        consignor.TryGetProperty("postalCode", out _).ShouldBeFalse("old field 'postalCode' should not exist");
        consignor.TryGetProperty("countryCode", out _).ShouldBeFalse("old field 'countryCode' should not exist");
        consignor.TryGetProperty("contactName", out _).ShouldBeFalse("contactName should not exist in API");
        consignor.TryGetProperty("state", out _).ShouldBeFalse("state should not exist in API");
    }

    [Fact]
    public async Task CreateShipmentAsync_ShipmentHasWeightAndPackageCount()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        // 2 packages: 5.0 kg + 5.25 kg = 10.25 kg total
        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var shipment = doc.RootElement.GetProperty("shipment");

        shipment.GetProperty("weight").GetString().ShouldBe("10.25");
        shipment.GetProperty("packageCount").GetString().ShouldBe("2");
    }

    [Fact]
    public async Task CreateShipmentAsync_PackagesHaveNOWeight()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var packages = doc.RootElement.GetProperty("packages");

        foreach (var pkg in packages.EnumerateArray())
        {
            pkg.TryGetProperty("weight", out _).ShouldBeFalse("weight should be on shipment level, not on package");
        }
    }

    [Fact]
    public async Task CreateShipmentAsync_CustomerReference_NotReference()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var shipment = doc.RootElement.GetProperty("shipment");

        shipment.TryGetProperty("customerReference", out var cr).ShouldBeTrue("should use customerReference");
        cr.GetString().ShouldBe("Test Reference");
        shipment.TryGetProperty("reference", out _).ShouldBeFalse("old field 'reference' should not exist");
    }

    [Fact]
    public async Task CreateShipmentAsync_MapsServiceCorrectly()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        var request = CreateTestRequest(service: GoExpressService.INT);
        await client.CreateShipmentAsync(request);

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"service\":\"INT\"");
    }

    [Fact]
    public async Task CreateShipmentAsync_MapsBooleanFieldsAsYesOrEmpty()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        var request = CreateTestRequest(selfPickup: true, identCheck: true);
        await client.CreateShipmentAsync(request);

        using var doc = await GetRequestBodyAsync(handler);
        var shipment = doc.RootElement.GetProperty("shipment");

        shipment.GetProperty("selfPickup").GetString().ShouldBe("Yes");
        shipment.GetProperty("identCheck").GetString().ShouldBe("Yes");
        shipment.GetProperty("freightCollect").GetString().ShouldBe("");
    }

    [Fact]
    public async Task CreateShipmentAsync_LabelFormatIsTopLevel()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest(labelFormat: GoExpressLabelFormat.Pdf4x6));

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        root.TryGetProperty("label", out var label).ShouldBeTrue("label should be top-level");
        label.GetString().ShouldBe("2");

        var shipment = root.GetProperty("shipment");
        shipment.TryGetProperty("labelFormat", out _).ShouldBeFalse("labelFormat should NOT be in shipment");
        shipment.TryGetProperty("label", out _).ShouldBeFalse("label should NOT be in shipment");
    }

    [Theory]
    [InlineData(WeekendOrHolidayIndicator.None, "")]
    [InlineData(WeekendOrHolidayIndicator.Saturday, "S")]
    [InlineData(WeekendOrHolidayIndicator.Holiday, "H")]
    public async Task CreateShipmentAsync_WeekendOrHolidayIndicator_MapsCorrectly(
        WeekendOrHolidayIndicator indicator, string expected)
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            Pickup = new TimeWindow
            {
                Date = new DateOnly(2025, 8, 2),
                TimeFrom = new TimeOnly(8, 0),
                TimeTill = new TimeOnly(12, 0),
                WeekendOrHolidayIndicator = indicator
            },
            Shipper = new Address { Name = "S", PostalCode = "12345", City = "C", CountryCode = "DE" },
            Consignee = new Address { Name = "R", PostalCode = "54321", City = "D", CountryCode = "DE" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        using var doc = await GetRequestBodyAsync(handler);
        var pickup = doc.RootElement.GetProperty("shipment").GetProperty("pickup");

        pickup.GetProperty("weekendOrHolidayIndicator").GetString().ShouldBe(expected);
        pickup.TryGetProperty("isWeekend", out _).ShouldBeFalse("old field isWeekend should not exist");
        pickup.TryGetProperty("isHoliday", out _).ShouldBeFalse("old field isHoliday should not exist");
    }

    [Fact]
    public async Task CreateShipmentAsync_ResponsibleStationAndCustomerId()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        await client.CreateShipmentAsync(CreateTestRequest());

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        root.GetProperty("responsibleStation").GetString().ShouldBe("HOQ");
        root.GetProperty("customerId").GetString().ShouldBe("50000");
    }

    [Fact]
    public async Task CreateShipmentAsync_NeutralAddress_WhenSet()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            IsNeutralPickup = true,
            NeutralAddress = new Address { Name = "Neutral GmbH", PostalCode = "10115", City = "Berlin", CountryCode = "DE" },
            Pickup = new TimeWindow { Date = new DateOnly(2025, 8, 1), TimeFrom = new TimeOnly(8, 0), TimeTill = new TimeOnly(12, 0) },
            Shipper = new Address { Name = "S", PostalCode = "12345", City = "C", CountryCode = "DE" },
            Consignee = new Address { Name = "R", PostalCode = "54321", City = "D", CountryCode = "DE" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        using var doc = await GetRequestBodyAsync(handler);
        var neutralAddr = doc.RootElement.GetProperty("neutralAddress");
        neutralAddr.GetProperty("name1").GetString().ShouldBe("Neutral GmbH");

        var shipment = doc.RootElement.GetProperty("shipment");
        shipment.GetProperty("isNeutralPickup").GetString().ShouldBe("Yes");
    }

    [Fact]
    public async Task CreateShipmentAsync_EmptyNeutralAddressAndDelivery_HaveAllFieldsAsEmptyStrings()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        // Request without NeutralAddress and without Delivery
        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            Pickup = new TimeWindow { Date = new DateOnly(2025, 8, 1), TimeFrom = new TimeOnly(8, 0), TimeTill = new TimeOnly(12, 0) },
            Shipper = new Address { Name = "S", PostalCode = "12345", City = "C", CountryCode = "DE" },
            Consignee = new Address { Name = "R", PostalCode = "54321", City = "D", CountryCode = "DE" },
            Packages = [new Package { Weight = 1.0 }]
        };

        await client.CreateShipmentAsync(request);

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        // neutralAddress should have all fields as empty strings, not missing
        var neutral = root.GetProperty("neutralAddress");
        neutral.GetProperty("name1").GetString().ShouldBe("");
        neutral.GetProperty("name2").GetString().ShouldBe("");
        neutral.GetProperty("name3").GetString().ShouldBe("");
        neutral.GetProperty("street").GetString().ShouldBe("");
        neutral.GetProperty("houseNumber").GetString().ShouldBe("");
        neutral.GetProperty("zipCode").GetString().ShouldBe("");
        neutral.GetProperty("city").GetString().ShouldBe("");
        neutral.GetProperty("country").GetString().ShouldBe("");

        // delivery should have all fields as empty strings, not missing
        var delivery = root.GetProperty("shipment").GetProperty("delivery");
        delivery.GetProperty("date").GetString().ShouldBe("");
        delivery.GetProperty("timeFrom").GetString().ShouldBe("");
        delivery.GetProperty("timeTill").GetString().ShouldBe("");
        delivery.GetProperty("avisFrom").GetString().ShouldBe("");
        delivery.GetProperty("avisTill").GetString().ShouldBe("");
        delivery.GetProperty("weekendOrHolidayIndicator").GetString().ShouldBe("");
    }

    // --- CancelShipment Tests ---

    [Fact]
    public async Task CancelShipmentAsync_Success_ReturnsSuccessResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await client.CancelShipmentAsync("GO1234567890");

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelShipmentAsync_IncludesStationAndCustomerId()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClientWithHandler(handler);

        await client.CancelShipmentAsync("GO1234567890");

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        body.ShouldContain("\"responsibleStation\":\"HOQ\"");
        body.ShouldContain("\"customerId\":\"50000\"");
        body.ShouldContain("\"hwbNumber\":\"GO1234567890\"");
        body.ShouldContain("\"orderStatus\":\"Cancelled\"");
    }

    [Fact]
    public async Task CancelShipmentAsync_NotFound_ReturnsFailureResult()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"HWB not found."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.CancelShipmentAsync("INVALID");

        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("HWB not found");
    }

    // --- GenerateLabel Tests ---

    [Fact]
    public async Task GenerateLabelAsync_ReturnsLabel()
    {
        var pdfBytes = "label-content"u8.ToArray();
        var base64 = Convert.ToBase64String(pdfBytes);
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(base64, System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.GenerateLabelAsync("GO1234567890");

        result.Format.ShouldBe(LabelFormat.Pdf);
        result.Content.ShouldBe(pdfBytes);
    }

    [Fact]
    public async Task GenerateLabelAsync_RequestUsesCorrectFieldNames()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Convert.ToBase64String("x"u8.ToArray()), System.Text.Encoding.UTF8, "application/json")
        });
        var client = CreateClientWithHandler(handler);

        await client.GenerateLabelAsync("346940009768", GoExpressLabelFormat.Pdf4x6);

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        root.GetProperty("responsibleStation").GetString().ShouldBe("HOQ");
        root.GetProperty("customerId").GetString().ShouldBe("50000");
        root.GetProperty("hwb").GetString().ShouldBe("346940009768");
        root.GetProperty("label").GetString().ShouldBe("2");

        // Old field names should not exist
        root.TryGetProperty("hwbNumber", out _).ShouldBeFalse("should use 'hwb' not 'hwbNumber'");
        root.TryGetProperty("labelFormat", out _).ShouldBeFalse("should use 'label' not 'labelFormat'");
    }

    // --- Unit Conversion Tests ---

    [Theory]
    [InlineData(1.0, WeightUnit.Kilogram, 1.0)]
    [InlineData(1000.0, WeightUnit.Gram, 1.0)]
    [InlineData(1.0, WeightUnit.Pound, 0.45359237)]
    [InlineData(1.0, WeightUnit.Ounce, 0.028349523)]
    public void ConvertWeight_ConvertsToKilogram(double value, WeightUnit unit, double expected)
    {
        var result = GoExpressShippingClient.ConvertWeight(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    [Theory]
    [InlineData(1.0, DimensionUnit.Centimeter, 1.0)]
    [InlineData(10.0, DimensionUnit.Millimeter, 1.0)]
    [InlineData(1.0, DimensionUnit.Inch, 2.54)]
    public void ConvertDimension_ConvertsToCentimeter(double value, DimensionUnit unit, double expected)
    {
        var result = GoExpressShippingClient.ConvertDimension(value, unit);
        result.ShouldBe(expected, 0.0001);
    }

    // --- Label Format Mapping Tests ---

    [Theory]
    [InlineData(GoExpressLabelFormat.Zpl, "1")]
    [InlineData(GoExpressLabelFormat.Pdf4x6, "2")]
    [InlineData(GoExpressLabelFormat.PdfA4, "4")]
    [InlineData(GoExpressLabelFormat.Tpcl, "5")]
    public void MapLabelFormat_ReturnsCorrectString(GoExpressLabelFormat format, string expected)
    {
        GoExpressShippingClient.MapLabelFormat(format).ShouldBe(expected);
    }

    // --- Integration-style: Doku-Example Payloads ---

    [Fact]
    public async Task CreateShipmentAsync_DokuExample_MatchesExpectedStructure()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                hwbNumber = "346940009768",
                orderStatus = "New",
                pickupDate = "01.08.2025",
                deliveryDate = "02.08.2025",
                transitInfo = new { datesVerified = "Yes", addressesVerified = "Yes", remarks = "" },
                hwbOrPackageLabel = Convert.ToBase64String("pdf"u8.ToArray()),
                package = new[]
                {
                    new { barcode = "346940009768001" },
                    new { barcode = "346940009768002" },
                    new { barcode = "346940009768003" }
                }
            })
        });
        var client = CreateClientWithHandler(handler);

        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            Reference = "Test Reference",
            Pickup = new TimeWindow
            {
                Date = new DateOnly(2025, 8, 1),
                TimeFrom = new TimeOnly(15, 0),
                TimeTill = new TimeOnly(17, 0)
            },
            Shipper = new Address
            {
                Name = "Test AG",
                Street = "Teststraße",
                HouseNumber = "5",
                PostalCode = "53119",
                City = "Bonn",
                CountryCode = "DE"
            },
            Consignee = new Address
            {
                Name = "Test Consignee 1",
                Street = "Empfängerweg",
                HouseNumber = "10",
                PostalCode = "36272",
                City = "Niederaula",
                CountryCode = "DE"
            },
            Packages =
            [
                new Package { Weight = 3.5, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } },
                new Package { Weight = 3.5, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } },
                new Package { Weight = 3.25, Dimensions = new Dimensions { Length = 30, Width = 20, Height = 15 } }
            ]
        };

        var result = await client.CreateShipmentAsync(request);

        result.ShipmentNumber.ShouldBe("346940009768");
        result.Labels.ShouldNotBeEmpty();

        // Verify the complete request structure
        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        // Top-level fields
        root.GetProperty("responsibleStation").GetString().ShouldBe("HOQ");
        root.GetProperty("customerId").GetString().ShouldBe("50000");
        root.GetProperty("label").GetString().ShouldBe("4"); // PdfA4

        // Shipment
        var shipment = root.GetProperty("shipment");
        shipment.GetProperty("service").GetString().ShouldBe("ON");
        shipment.GetProperty("weight").GetString().ShouldBe("10.25"); // 3.5 + 3.5 + 3.25
        shipment.GetProperty("packageCount").GetString().ShouldBe("3");
        shipment.GetProperty("customerReference").GetString().ShouldBe("Test Reference");

        // Pickup dates in dd.MM.yyyy format
        shipment.GetProperty("pickup").GetProperty("date").GetString().ShouldBe("01.08.2025");
        shipment.GetProperty("pickup").GetProperty("timeFrom").GetString().ShouldBe("15:00");
        shipment.GetProperty("pickup").GetProperty("timeTill").GetString().ShouldBe("17:00");

        // Addresses at top-level with correct field names
        var consignor = root.GetProperty("consignorAddress");
        consignor.GetProperty("name1").GetString().ShouldBe("Test AG");
        consignor.GetProperty("zipCode").GetString().ShouldBe("53119");
        consignor.GetProperty("country").GetString().ShouldBe("DE");

        var consignee = root.GetProperty("consigneeAddress");
        consignee.GetProperty("name1").GetString().ShouldBe("Test Consignee 1");
        consignee.GetProperty("zipCode").GetString().ShouldBe("36272");

        // Packages at top-level, no weight
        var packages = root.GetProperty("packages");
        packages.GetArrayLength().ShouldBe(3);
        foreach (var pkg in packages.EnumerateArray())
        {
            pkg.TryGetProperty("weight", out _).ShouldBeFalse();
            pkg.TryGetProperty("length", out _).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task CreateShipmentAsync_WithOptionalFields_SelfDeliveryAndCashOnDelivery()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateSuccessResponse())
        });
        var client = CreateClientWithHandler(handler);

        var request = new GoExpressShipmentRequest
        {
            Service = GoExpressService.ON,
            SelfDelivery = true,
            CashOnDeliveryAmount = 150.00m,
            CashOnDeliveryCurrency = "EUR",
            Dimensions = "120x80x60",
            IsNeutralPickup = true,
            NeutralAddress = new Address { Name = "Neutral Corp", PostalCode = "10115", City = "Berlin", CountryCode = "DE" },
            Pickup = new TimeWindow { Date = new DateOnly(2025, 8, 1), TimeFrom = new TimeOnly(8, 0), TimeTill = new TimeOnly(12, 0) },
            Shipper = new Address { Name = "S", PostalCode = "12345", City = "C", CountryCode = "DE" },
            Consignee = new Address { Name = "R", PostalCode = "54321", City = "D", CountryCode = "DE" },
            Packages = [new Package { Weight = 5.0 }]
        };

        await client.CreateShipmentAsync(request);

        using var doc = await GetRequestBodyAsync(handler);
        var shipment = doc.RootElement.GetProperty("shipment");

        shipment.GetProperty("selfDelivery").GetString().ShouldBe("Yes");
        shipment.GetProperty("cashOnDelivery").GetProperty("amount").GetString().ShouldBe("150.00");
        shipment.GetProperty("cashOnDelivery").GetProperty("currency").GetString().ShouldBe("EUR");
        shipment.GetProperty("dimensions").GetString().ShouldBe("120x80x60");
        shipment.GetProperty("isNeutralPickup").GetString().ShouldBe("Yes");

        doc.RootElement.GetProperty("neutralAddress").GetProperty("name1").GetString().ShouldBe("Neutral Corp");
    }

    [Fact]
    public async Task GenerateLabelAsync_DokuExample_MatchesExpectedRequest()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Convert.ToBase64String("label"u8.ToArray()), System.Text.Encoding.UTF8, "application/json")
        });
        var client = CreateClientWithHandler(handler);

        await client.GenerateLabelAsync("346940009768", GoExpressLabelFormat.Pdf4x6);

        using var doc = await GetRequestBodyAsync(handler);
        var root = doc.RootElement;

        root.GetProperty("responsibleStation").GetString().ShouldBe("HOQ");
        root.GetProperty("customerId").GetString().ShouldBe("50000");
        root.GetProperty("hwb").GetString().ShouldBe("346940009768");
        root.GetProperty("label").GetString().ShouldBe("2");
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_response);
    }
}
