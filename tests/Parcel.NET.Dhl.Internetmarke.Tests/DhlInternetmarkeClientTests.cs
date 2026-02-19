using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internetmarke.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Internetmarke.Tests;

public class DhlInternetmarkeClientTests
{
    private static DhlInternetmarkeClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-eu.dhl.com/post/de/shipping/im/v1/")
        });

    private static MockHttpMessageHandler CreateHandler(HttpResponseMessage response) =>
        new(response);

    private static DhlInternetmarkeClient CreateClient(MockHttpMessageHandler handler) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-eu.dhl.com/post/de/shipping/im/v1/")
        });

    // --- GetUserProfileAsync ---

    [Fact]
    public async Task GetUserProfileAsync_ReturnsProfile()
    {
        var responseBody = new
        {
            ekp = "1234567890",
            company = "Test GmbH",
            salutation = "Herr",
            title = "Dr.",
            mail = "max@test.de",
            firstname = "Max",
            lastname = "Mustermann",
            street = "Teststr.",
            houseNo = "42",
            zip = "53113",
            city = "Bonn",
            country = "DEU"
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetUserProfileAsync();

        result.Ekp.ShouldBe("1234567890");
        result.Company.ShouldBe("Test GmbH");
        result.Email.ShouldBe("max@test.de");
        result.Firstname.ShouldBe("Max");
        result.Lastname.ShouldBe("Mustermann");
        result.Street.ShouldBe("Teststr.");
        result.HouseNo.ShouldBe("42");
        result.Zip.ShouldBe("53113");
        result.City.ShouldBe("Bonn");
        result.Country.ShouldBe("DEU");
    }

    [Fact]
    public async Task GetUserProfileAsync_RequestUrl_IsUserProfile()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { ekp = "123", mail = "test@test.de" })
        });
        var client = CreateClient(handler);

        await client.GetUserProfileAsync();

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.ToString().ShouldEndWith("user/profile");
    }

    [Fact]
    public async Task GetUserProfileAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("""{"detail":"Invalid credentials."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.GetUserProfileAsync());
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // --- GetCatalogAsync ---

    [Fact]
    public async Task GetCatalogAsync_ReturnsCatalogResult()
    {
        var responseBody = new
        {
            pageFormats = new[]
            {
                new { id = 1, name = "DIN A4", description = "Standard A4", pageType = "REGULARPAGE", isAddressPossible = true, isImagePossible = false }
            },
            contractProducts = new[]
            {
                new { productCode = 10001, price = 85 },
                new { productCode = 10002, price = 100 }
            },
            publicCatalog = new
            {
                items = new[]
                {
                    new { category = "Nature", categoryDescription = "Nature images", categoryId = 1, images = new[] { "img1.jpg" } }
                }
            },
            privateCatalog = new
            {
                imageLink = new[] { "private1.jpg" }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetCatalogAsync();

        result.PageFormats.Count.ShouldBe(1);
        result.PageFormats[0].Name.ShouldBe("DIN A4");
        result.PageFormats[0].IsAddressPossible.ShouldBeTrue();
        result.ContractProducts.Count.ShouldBe(2);
        result.ContractProducts[0].ProductCode.ShouldBe(10001);
        result.ContractProducts[0].Price.ShouldBe(85);
        result.PublicCatalogItems.Count.ShouldBe(1);
        result.PublicCatalogItems[0].Category.ShouldBe("Nature");
        result.PrivateCatalogImageLinks.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetCatalogAsync_WithTypes_AppendQueryParam()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { pageFormats = Array.Empty<object>() })
        });
        var client = CreateClient(handler);

        await client.GetCatalogAsync("PAGE_FORMATS,CONTRACT_PRODUCTS");

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.RequestUri!.ToString().ShouldContain("app/catalog?types=");
        handler.LastRequest.RequestUri.ToString().ShouldContain("PAGE_FORMATS");
    }

    [Fact]
    public async Task GetCatalogAsync_NullDeserialization_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        await Should.ThrowAsync<ParcelException>(() => client.GetCatalogAsync());
    }

    // --- InitializeCartAsync ---

    [Fact]
    public async Task InitializeCartAsync_ReturnsShopOrderId()
    {
        var responseBody = new { shopOrderId = "SO-12345" };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.InitializeCartAsync();

        result.ShopOrderId.ShouldBe("SO-12345");
    }

    [Fact]
    public async Task InitializeCartAsync_PostsWithNoBody()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { shopOrderId = "SO-001" })
        });
        var client = CreateClient(handler);

        await client.InitializeCartAsync();

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/shoppingcart");
    }

    // --- GetCartAsync ---

    [Fact]
    public async Task GetCartAsync_ReturnsCart()
    {
        var responseBody = new { shopOrderId = "SO-12345" };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetCartAsync("SO-12345");

        result.ShopOrderId.ShouldBe("SO-12345");
    }

    [Fact]
    public async Task GetCartAsync_EmptyId_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.GetCartAsync(""));
    }

    // --- CheckoutCartPdfAsync ---

    [Fact]
    public async Task CheckoutCartPdfAsync_ReturnsCheckoutResult()
    {
        var responseBody = new
        {
            link = "https://example.com/stamps.pdf",
            manifestLink = (string?)null,
            shoppingCart = new
            {
                shopOrderId = "SO-12345",
                voucherList = new[]
                {
                    new { voucherId = "V-001", trackId = "T-001" },
                    new { voucherId = "V-002", trackId = "" }
                }
            },
            walletBallance = 9150,
            type = "AppShoppingCartPDFResponse"
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = new CheckoutRequest
        {
            ShopOrderId = "SO-12345",
            Total = 850,
            PageFormatId = 1,
            Positions =
            [
                new CheckoutPosition
                {
                    ProductCode = 10001,
                    ImageId = 0,
                    Sender = new InternetmarkeAddress
                    {
                        Name = "Max Mustermann",
                        AddressLine1 = "Teststr. 1",
                        PostalCode = "53113",
                        City = "Bonn"
                    },
                    Receiver = new InternetmarkeAddress
                    {
                        Name = "Erika Musterfrau",
                        AddressLine1 = "Hauptstr. 2",
                        PostalCode = "10117",
                        City = "Berlin"
                    }
                }
            ]
        };

        var result = await client.CheckoutCartPdfAsync(request);

        result.Link.ShouldBe("https://example.com/stamps.pdf");
        result.ShopOrderId.ShouldBe("SO-12345");
        result.Vouchers.Count.ShouldBe(2);
        result.Vouchers[0].VoucherId.ShouldBe("V-001");
        result.Vouchers[0].TrackId.ShouldBe("T-001");
        result.WalletBalanceCents.ShouldBe(9150);
    }

    [Fact]
    public async Task CheckoutCartPdfAsync_RequestUrl_ContainsPdfEndpoint()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                link = "https://example.com/stamps.pdf",
                shoppingCart = new { shopOrderId = "SO-001", voucherList = Array.Empty<object>() },
                walletBallance = 1000,
                type = "AppShoppingCartPDFResponse"
            })
        });
        var client = CreateClient(handler);

        var request = new CheckoutRequest
        {
            ShopOrderId = "SO-001",
            Total = 85,
            PageFormatId = 1,
            Positions = [new CheckoutPosition { ProductCode = 10001 }]
        };

        await client.CheckoutCartPdfAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/shoppingcart/pdf");
        handler.LastRequest.RequestUri.ToString().ShouldContain("directCheckout=true");
    }

    [Fact]
    public async Task CheckoutCartPdfAsync_NullRequest_ThrowsArgumentNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.CheckoutCartPdfAsync(null!));
    }

    // --- CheckoutCartPngAsync ---

    [Fact]
    public async Task CheckoutCartPngAsync_RequestUrl_ContainsPngEndpoint()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                link = "https://example.com/stamps.png",
                shoppingCart = new { shopOrderId = "SO-001", voucherList = Array.Empty<object>() },
                walletBallance = 1000,
                type = "AppShoppingCartPNGResponse"
            })
        });
        var client = CreateClient(handler);

        var request = new CheckoutRequest
        {
            ShopOrderId = "SO-001",
            Total = 85,
            PageFormatId = 1,
            Dpi = "DPI600",
            Positions = [new CheckoutPosition { ProductCode = 10001 }]
        };

        await client.CheckoutCartPngAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/shoppingcart/png");
        handler.LastRequest.RequestUri.ToString().ShouldContain("directCheckout=true");
    }

    // --- ChargeWalletAsync ---

    [Fact]
    public async Task ChargeWalletAsync_SendsPutRequest()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(handler);

        var request = new WalletChargeRequest { Amount = 1000, PaymentSystem = "SEPA_DIRECT_DEBIT" };
        await client.ChargeWalletAsync(request);

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/wallet");
    }

    [Fact]
    public async Task ChargeWalletAsync_NullRequest_ThrowsArgumentNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.ChargeWalletAsync(null!));
    }

    // --- RequestRetoureAsync ---

    [Fact]
    public async Task RequestRetoureAsync_ReturnsResult()
    {
        var responseBody = new { retoureId = "RET-001", shopRetoureId = "SHOP-RET-001" };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = new RetoureRequest
        {
            ShopRetoureId = "SHOP-RET-001",
            VoucherIds = ["V-001", "V-002"]
        };

        var result = await client.RequestRetoureAsync(request);

        result.RetoureId.ShouldBe("RET-001");
        result.ShopRetoureId.ShouldBe("SHOP-RET-001");
    }

    [Fact]
    public async Task RequestRetoureAsync_PostsToRetoureEndpoint()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { retoureId = "RET-001" })
        });
        var client = CreateClient(handler);

        await client.RequestRetoureAsync(new RetoureRequest { VoucherIds = ["V-001"] });

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/retoure");
    }

    // --- GetRetoureStateAsync ---

    [Fact]
    public async Task GetRetoureStateAsync_ReturnsState()
    {
        var responseBody = new
        {
            retoureId = "RET-001",
            shopRetoureId = "SHOP-RET-001",
            retoureState = "COMPLETED",
            retoureTransactionId = "TXN-001"
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetRetoureStateAsync();

        result.RetoureId.ShouldBe("RET-001");
        result.State.ShouldBe("COMPLETED");
        result.RetoureTransactionId.ShouldBe("TXN-001");
    }

    [Fact]
    public async Task GetRetoureStateAsync_RequestUrl_IsAppRetoure()
    {
        var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { retoureId = "RET-001", retoureState = "PENDING" })
        });
        var client = CreateClient(handler);

        await client.GetRetoureStateAsync();

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.ToString().ShouldContain("app/retoure");
    }

    // --- Error handling ---

    [Fact]
    public async Task CheckoutCartPdfAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"detail":"Invalid request data."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var request = new CheckoutRequest
        {
            ShopOrderId = "SO-001",
            Total = 85,
            PageFormatId = 1,
            Positions = [new CheckoutPosition { ProductCode = 10001 }]
        };

        var ex = await Should.ThrowAsync<ParcelException>(() => client.CheckoutCartPdfAsync(request));
        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // --- walletBallance typo matching ---

    [Fact]
    public async Task CheckoutCartPdfAsync_MatchesWalletBallanceTypo()
    {
        // The official DHL API has a typo: "walletBallance" with double L
        var responseJson = """
        {
            "link": "https://example.com/stamps.pdf",
            "shoppingCart": { "shopOrderId": "SO-001", "voucherList": [] },
            "walletBallance": 7777,
            "type": "AppShoppingCartPDFResponse"
        }
        """;

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
        });

        var request = new CheckoutRequest
        {
            ShopOrderId = "SO-001",
            Total = 85,
            PageFormatId = 1,
            Positions = [new CheckoutPosition { ProductCode = 10001 }]
        };

        var result = await client.CheckoutCartPdfAsync(request);
        result.WalletBalanceCents.ShouldBe(7777);
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
